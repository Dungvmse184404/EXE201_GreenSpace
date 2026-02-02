using AutoMapper;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.DTOs.User;
using GreenSpace.Application.Enums;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Security;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Constants;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;


namespace GreenSpace.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordHashService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly IOtpService _otpService;
        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordService passwordHashService,
            ITokenService tokenService,
            ILogger<AuthService> logger,
            IMapper mapper,
            IDistributedCache cache,
            IOtpService otpService)
        {
            _unitOfWork = unitOfWork;
            _passwordHashService = passwordHashService;
            _tokenService = tokenService;
            _logger = logger;
            _mapper = mapper;
            _cache = cache;
            _otpService = otpService;
        }


        /// <summary>
        /// Khởi tạo đăng ký - Kiểm tra email và gửi OTP
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        public async Task<IServiceResult> RegisterMailAsync(RegisteMailDto dto)
        {
            try
            {
                var exists = await _unitOfWork.UserRepository.EmailExistsAsync(dto.Email);
                if (exists)
                {
                    _logger?.LogWarning("Register init failed: Email {Email} already exists", dto.Email);
                    return ServiceResult.Failure(ApiStatusCodes.Conflict, ApiMessages.Auth.ExistedEmail);
                }

                await _otpService.SendOtpAsync(dto.Email, "Mã xác thực đăng ký GreenSpace", "Register");

                return ServiceResult.Success(ApiMessages.OTP.Sent);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initiate register for {Email}", dto.Email);
                return ServiceResult.Failure(ApiStatusCodes.InternalServerError, ApiMessages.OTP.SentFailed);
            }
        }


        public async Task<IServiceResult> VerifyRegisterOtpAsync(VerifyRegisterOtpDto dto)
        {
            var isValid = await _otpService.VerifyOtpAsync(dto.Email, dto.Otp, "Register");

            switch (isValid)
            {
                case OtpResult.Invalid:
                    return ServiceResult.Failure(ApiStatusCodes.BadRequest, ApiMessages.OTP.Invalid);

                case OtpResult.Expired:
                    return ServiceResult.Failure(ApiStatusCodes.Gone, ApiMessages.OTP.Expired);
            }

            var verifiedKey = $"PreRegisterVerified:{dto.Email}";

            await _cache.SetStringAsync(verifiedKey, "true", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
            });

            return ServiceResult.Success(ApiMessages.OTP.Verified); // "Xác thực thành công"
        }



        /// <summary>`
        /// Authenticate user with email and password
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>Authentication result</returns>
        public async Task<IServiceResult<AuthResultDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _unitOfWork.UserRepository.GetByEmailAsync(loginDto.Email);

                if (user == null || !_passwordHashService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed for: {Email}", loginDto.Email);
                    return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.Unauthorized, ApiMessages.Auth.LoginFailed);
                }

                if (user.IsActive != true)
                {
                    return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.NotFound, ApiMessages.Auth.AccountInactive);
                }

                string jti = Guid.NewGuid().ToString();

                // Tạo Access Token
                var accessToken = _tokenService.GenerateAccessToken(user.UserId, user.Email, user.Role, jti);

                var refreshTokenEntity = await _unitOfWork.RefreshTokenService.GenerateRefreshTokenAsync(user.UserId, accessToken, jti);

                var authResult = _mapper.Map<AuthResultDto>(user);
                authResult.AccessToken = accessToken;
                authResult.RefreshToken = refreshTokenEntity.Token;
                authResult.ExpiresAt = _tokenService.GetTokenExpiration();

                return ServiceResult<AuthResultDto>.Success(authResult, ApiMessages.Auth.LoginSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for: {Email}", loginDto.Email);
                return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.InternalServerError, ApiMessages.Error.General);
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="registerDto">Registration data</param>
        /// <returns>Authentication result</returns>
        public async Task<IServiceResult<UserDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Kiểm tra xem email này đã verify OTP chưa
                var verifiedKey = $"PreRegisterVerified:{registerDto.Email}";
                var isVerified = await _cache.GetStringAsync(verifiedKey);

                if (string.IsNullOrEmpty(isVerified))
                {
                    return ServiceResult<UserDto>.Failure(ApiStatusCodes.Unauthorized, "Phiên làm việc hết hạn hoặc chưa xác thực OTP.");
                }

                if (await _unitOfWork.UserRepository.EmailExistsAsync(registerDto.Email))
                {
                    return ServiceResult<UserDto>.Failure(ApiStatusCodes.Conflict, ApiMessages.Mail.Existed);
                }

                if (await _unitOfWork.UserRepository.PhoneExistsAsync(registerDto.PhoneNumber))
                {
                    return ServiceResult<UserDto>.Failure(ApiStatusCodes.Conflict, ApiMessages.Mail.PhoneExisted);
                }

                var newUser = _mapper.Map<User>(registerDto);
                newUser.Role = Roles.Customer;
                newUser.PasswordHash = _passwordHashService.HashPassword(registerDto.Password);
                newUser.IsActive = true;

                await _unitOfWork.UserRepository.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                await _cache.RemoveAsync(verifiedKey);

                return ServiceResult<UserDto>.Success(_mapper.Map<UserDto>(newUser), ApiMessages.User.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for: {Email}", registerDto.Email);
                return ServiceResult<UserDto>.Failure(ApiStatusCodes.InternalServerError, ApiMessages.Error.General);
            }
        }


        /// <summary>
        /// Create a new staff member (Admin only)
        /// </summary>
        /// <param name="createInternalDto">Staff creation data</param>
        /// <returns>Authentication result</returns>
        public async Task<IServiceResult<UserDto>> CreateInternalUserAsync(InternalUserDto createInternalDto)
        {
            try
            {
                if (await _unitOfWork.UserRepository.EmailExistsAsync(createInternalDto.Email))
                {
                    return ServiceResult<UserDto>.Failure(ApiStatusCodes.Conflict, ApiMessages.Mail.Existed);
                }

                var normalizedRole = Roles.Normalize(createInternalDto.Role);
                if (normalizedRole == null)
                {
                    return ServiceResult<UserDto>.Failure(ApiStatusCodes.BadRequest, $"{ApiMessages.role.NotFound}.");
                }

                var newUser = _mapper.Map<User>(createInternalDto);
                newUser.PasswordHash = _passwordHashService.HashPassword(createInternalDto.Password);
                newUser.IsActive = true;

                await _unitOfWork.UserRepository.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<UserDto>.Success(_mapper.Map<UserDto>(newUser), ApiMessages.User.Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating internal user: {Email}", createInternalDto.Email);
                return ServiceResult<UserDto>.Failure(ApiStatusCodes.InternalServerError, ApiMessages.User.CreateFailed);
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="accessToken">Current access token to validate</param>
        /// <returns>New authentication result</returns>
        /// <summary>
        /// Refresh access token with proper rotation
        /// </summary>
        public async Task<IServiceResult<AuthResultDto>> RefreshTokenAsync(
            string refreshToken,
            string accessToken)
        {
            try
            {
                _logger?.LogInformation("Token refresh attempt started");

                // Validate access token structure (ignore expiration)
                var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {
                    _logger?.LogWarning("Invalid access token structure");
                    return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.Unauthorized, "Invalid access token");
                }

                // Extract user ID from token
                var userIdClaim = principal.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger?.LogWarning("Invalid user ID in token");
                    return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.Unauthorized, "Invalid token claims");
                }

                // Validate refresh token with access token hash
                var refreshTokenEntity = await _unitOfWork.RefreshTokenService
                    .GetRefreshTokenWithAccessTokenValidationAsync(refreshToken, accessToken);

                if (refreshTokenEntity == null)
                {
                    _logger?.LogWarning(
                        "Refresh token validation failed for user: {UserId}",
                        userId);
                    return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.Unauthorized, "Invalid or expired refresh token");
                }

                // Get user with role
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null || user.IsActive != true)
                {
                    _logger?.LogWarning(
                        "User not found or inactive for refresh token: {UserId}",
                        userId);

                    // Revoke compromised token
                    await _unitOfWork.RefreshTokenService.RevokeRefreshTokenAsync(refreshToken);

                    return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.Forbidden, "User account is not active");
                }

                // Begin transaction for token rotation
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Mark old refresh token as used
                    refreshTokenEntity.IsUsed = true;
                    await _unitOfWork.RefreshTokenRepository.UpdateAsync(refreshTokenEntity);

                    string jwtId = Guid.NewGuid().ToString();
                    // Generate new tokens
                    var newAccessToken = _tokenService.GenerateAccessToken(
                        user.UserId,
                        user.Email,
                        user.Role,
                        jwtId);

                    var newRefreshTokenEntity = await _unitOfWork.RefreshTokenService
                        .GenerateRefreshTokenAsync(user.UserId, newAccessToken, jwtId ?? Guid.NewGuid().ToString());

                    // Save changes and commit transaction
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();

                    // Create response
                    var authResult = _mapper.Map<AuthResultDto>(user);
                    authResult.AccessToken = newAccessToken;
                    authResult.RefreshToken = newRefreshTokenEntity.Id.ToString();
                    authResult.ExpiresAt = _tokenService.GetTokenExpiration();

                    _logger?.LogInformation(
                        "Token refreshed successfully for user: {UserId}",
                        user.UserId);

                    return ServiceResult<AuthResultDto>.Success(
                        authResult,
                        "Token refreshed successfully");
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    _logger?.LogError(ex, "Error during token refresh transaction");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during token refresh");
                return ServiceResult<AuthResultDto>.Failure(
                    ApiStatusCodes.InternalServerError,
                    $"Error during token refresh: {ex.Message}");
            }
        }

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token to revoke</param>
        /// <returns>Success status</returns>
        public async Task<IServiceResult<bool>> RevokeTokenAsync(string refreshToken)
        {
            var result = await _unitOfWork.RefreshTokenService.RevokeRefreshTokenAsync(refreshToken);
            if (!result) return ServiceResult<bool>.Failure(ApiStatusCodes.BadRequest, "Token không tồn tại.");

            return ServiceResult<bool>.Success(true, "Đã thu hồi token.");
        }

        /// <summary>
        /// Revoke all refresh tokens for a user
        /// </summary>
        /// <param name="userId">Useraccount ID</param>
        /// <returns>Success status</returns>
        public async Task<IServiceResult<bool>> RevokeAllUserTokensAsync(Guid userId)
        {
            await _unitOfWork.RefreshTokenService.RevokeAllUserRefreshTokensAsync(userId);
            return ServiceResult<bool>.Success(true, "Đã thu hồi toàn bộ token của người dùng.");
        }



    }
}