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
                    return ServiceResult.Failure(ApiMessages.Auth.ExistedEmail);
                }

                await _otpService.SendOtpAsync(dto.Email, "Mã xác thực đăng ký GreenSpace", "Register");

                return ServiceResult.Success(ApiMessages.OTP.Sent);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initiate register for {Email}", dto.Email);
                return ServiceResult.Failure("Lỗi hệ thống khi gửi OTP");
            }
        }


        public async Task<IServiceResult> VerifyRegisterOtpAsync(VerifyRegisterOtpDto dto)
        {
            var isValid = await _otpService.VerifyOtpAsync(dto.Email, dto.Otp, "Register");

            switch (isValid)
            {
                case OtpResult.Invalid:
                    return ServiceResult.Failure(ApiMessages.OTP.Invalid);

                case OtpResult.Expired:
                    return ServiceResult.Failure(ApiMessages.OTP.Expired);
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
                // Get user with role information
                var user = await _unitOfWork.UserRepository.GetByEmailWithRoleAsync(loginDto.Email);

                if (user == null)
                {
                    _logger?.LogWarning("Login attempt failed for email: {Email} - Useraccount not found", loginDto.Email);
                    return ServiceResult<AuthResultDto>.Failure("Invalid email or password");
                }

                if (user.IsActive != true)
                {
                    _logger?.LogWarning("Login attempt failed for email: {Email} - Useraccount is inactive", loginDto.Email);
                    return ServiceResult<AuthResultDto>.Failure("Useraccount account is inactive");
                }

                // Verify password
                if (!_passwordHashService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger?.LogWarning("Login attempt failed for email: {Email} - Invalid password", loginDto.Email);
                    return ServiceResult<AuthResultDto>.Failure("Invalid email or password");
                }

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(user.UserId, user.Email, user.Role);
                var refreshTokenEntity = await _unitOfWork.RefreshTokenService.GenerateRefreshTokenAsync(user.UserId, accessToken);
                var expiresAt = _tokenService.GetTokenExpiration();

                // Create auth result (AutoMapper)
                var authResult = _mapper.Map<AuthResultDto>(user);
                authResult.AccessToken = accessToken;
                authResult.RefreshToken = refreshTokenEntity.Token;

                authResult.ExpiresAt = expiresAt;

                _logger?.LogInformation("Useraccount logged in successfully: {Email}", loginDto.Email);
                return ServiceResult<AuthResultDto>.Success(authResult, "Login successful");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during login for email: {Email}", loginDto.Email);
                return ServiceResult<AuthResultDto>.Failure($"Error during login: {ex.Message}");
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
                var verifiedKey = $"PreRegisterVerified:{registerDto.Email}";
                var isVerified = await _cache.GetStringAsync(verifiedKey);

                if (string.IsNullOrEmpty(isVerified))
                {
                    _logger?.LogWarning("Registration failed: Email {Email} not verified via OTP", registerDto.Email);
                    return ServiceResult<UserDto>.Failure("Bạn chưa xác thực email hoặc phiên đăng ký đã hết hạn. Vui lòng thử lại.");
                }

                // Kiểm tra lại Email (Double check - phòng trường hợp race condition)
                if (await _unitOfWork.UserRepository.EmailExistsAsync(registerDto.Email))
                {
                    return ServiceResult<UserDto>.Failure("Email này đã được đăng ký bởi người khác.");
                }

                if (await _unitOfWork.UserRepository.PhoneExistsAsync(registerDto.PhoneNumber))
                {
                    return ServiceResult<UserDto>.Failure("Số điện thoại đã tồn tại.");
                }

                var newUser = _mapper.Map<User>(registerDto);

                newUser.Role = Roles.Customer;
                newUser.PasswordHash = _passwordHashService.HashPassword(registerDto.Password);

                newUser.IsActive = true;

                await _unitOfWork.UserRepository.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                await _cache.RemoveAsync(verifiedKey);

                var userDto = _mapper.Map<UserDto>(newUser);
                _logger?.LogInformation("User registered successfully: {Email}", registerDto.Email);

                return ServiceResult<UserDto>.Success(userDto, "Đăng ký thành công.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
                return ServiceResult<UserDto>.Failure($"Lỗi khi đăng ký: {ex.Message}");
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
                    return ServiceResult<UserDto>.Failure(ApiMessages.Mail.Existed);
                }

                var normalizedRole = Roles.Normalize(createInternalDto.Role);
                if (normalizedRole == null)
                {
                    return ServiceResult<UserDto>.Failure($"{ApiMessages.role.NotFound}. Các role cho phép: {string.Join(", ", Roles.All)}");
                }

                var newUser = _mapper.Map<User>(createInternalDto);
                newUser.Role = createInternalDto.Role;
                newUser.PasswordHash = _passwordHashService.HashPassword(createInternalDto.Password);

                // Admin tạo thì auto active
                newUser.IsActive = true;

                await _unitOfWork.UserRepository.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(newUser);
                return ServiceResult<UserDto>.Success(userDto, ApiMessages.User.Created);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating internal user: {Email}", createInternalDto.Email);
                return ServiceResult<UserDto>.Failure($"{ApiMessages.User.CreateFailed}: {ex.Message}");
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="accessToken">Current access token to validate</param>
        /// <returns>New authentication result</returns>
        public async Task<IServiceResult<AuthResultDto>> RefreshTokenAsync(string refreshToken, string accessToken)
        {
            try
            {
                var refreshTokenEntity = await _unitOfWork.RefreshTokenService.GetRefreshTokenWithAccessTokenValidationAsync(refreshToken, accessToken);

                if (refreshTokenEntity == null)
                {
                    _logger?.LogWarning("Refresh token invalid/expired or mismatch: {RefreshToken}", refreshToken);
                    return ServiceResult<AuthResultDto>.Failure("Invalid or expired refresh token");
                }

                var user = await _unitOfWork.UserRepository.GetByIdAsync(refreshTokenEntity.UserId);

                if (user == null || user.IsActive != true)
                {
                    _logger?.LogWarning("User not found/inactive for refresh token: {UserId}", refreshTokenEntity.UserId);
                    // Revoke token để bảo mật
                    await _unitOfWork.RefreshTokenService.RevokeRefreshTokenAsync(refreshToken);
                    return ServiceResult<AuthResultDto>.Failure("User account is not active");
                }

                var newAccessToken = _tokenService.GenerateAccessToken(user.UserId, user.Email, user.Role);
                var newRefreshTokenEntity = await _unitOfWork.RefreshTokenService.GenerateRefreshTokenAsync(user.UserId, newAccessToken);
                var expiresAt = _tokenService.GetTokenExpiration();

                await _unitOfWork.RefreshTokenService.RevokeRefreshTokenAsync(refreshToken);

                var authResult = _mapper.Map<AuthResultDto>(user);

                authResult.AccessToken = newAccessToken;
                authResult.RefreshToken = newRefreshTokenEntity.Token;
                authResult.ExpiresAt = expiresAt;

                _logger?.LogInformation("Token refreshed successfully for user: {UserId}", user.UserId);
                return ServiceResult<AuthResultDto>.Success(authResult, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during token refresh");
                return ServiceResult<AuthResultDto>.Failure($"Error during token refresh: {ex.Message}");
            }
        }

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token to revoke</param>
        /// <returns>Success status</returns>
        public async Task<IServiceResult<bool>> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var result = await _unitOfWork.RefreshTokenService.RevokeRefreshTokenAsync(refreshToken);

                if (!result)
                {
                    _logger?.LogWarning("Attempted to revoke non-existent refresh token");
                    return ServiceResult<bool>.Failure("Invalid refresh token");
                }

                _logger?.LogInformation("Refresh token revoked successfully");
                return ServiceResult<bool>.Success(true, "Token revoked successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during token revocation");
                return ServiceResult<bool>.Failure($"Error during token revocation: {ex.Message}");
            }
        }

        /// <summary>
        /// Revoke all refresh tokens for a user
        /// </summary>
        /// <param name="userId">Useraccount ID</param>
        /// <returns>Success status</returns>
        public async Task<IServiceResult<bool>> RevokeAllUserTokensAsync(Guid userId)
        {
            try
            {
                var result = await _unitOfWork.RefreshTokenService.RevokeAllUserRefreshTokensAsync(userId);

                if (!result)
                {
                    _logger?.LogInformation("No tokens found to revoke for user: {UserId}", userId);
                    return ServiceResult<bool>.Success(true, "No tokens to revoke");
                }

                _logger?.LogInformation("All refresh tokens revoked successfully for user: {UserId}", userId);
                return ServiceResult<bool>.Success(true, "All tokens revoked successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during all tokens revocation for user: {UserId}", userId);
                return ServiceResult<bool>.Failure($"Error during tokens revocation: {ex.Message}");
            }
        }

    }
}