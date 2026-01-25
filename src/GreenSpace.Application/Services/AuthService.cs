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
                var user = await _unitOfWork.UserRepository.GetByEmailWithRoleAsync(loginDto.Email);

                if (user == null || !_passwordHashService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed for: {Email}", loginDto.Email);
                    return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.Unauthorized, "Email hoặc mật khẩu không chính xác");
                }

                if (user.IsActive != true)
                {
                    return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.Forbidden, "Tài khoản của bạn đã bị khóa.");
                }

                var accessToken = _tokenService.GenerateAccessToken(user.UserId, user.Email, user.Role);
                var refreshTokenEntity = await _unitOfWork.RefreshTokenService.GenerateRefreshTokenAsync(user.UserId, accessToken);

                var authResult = _mapper.Map<AuthResultDto>(user);
                authResult.AccessToken = accessToken;
                authResult.RefreshToken = refreshTokenEntity.Token;
                authResult.ExpiresAt = _tokenService.GetTokenExpiration();

                return ServiceResult<AuthResultDto>.Success(authResult, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for: {Email}", loginDto.Email);
                return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.InternalServerError, "Lỗi hệ thống khi đăng nhập.");
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
                    return ServiceResult<UserDto>.Failure(ApiStatusCodes.Conflict, "Email đã được sử dụng.");
                }

                if (await _unitOfWork.UserRepository.PhoneExistsAsync(registerDto.PhoneNumber))
                {
                    return ServiceResult<UserDto>.Failure(ApiStatusCodes.Conflict, "Số điện thoại đã được sử dụng.");
                }

                var newUser = _mapper.Map<User>(registerDto);
                newUser.Role = Roles.Customer;
                newUser.PasswordHash = _passwordHashService.HashPassword(registerDto.Password);
                newUser.IsActive = true;

                await _unitOfWork.UserRepository.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                await _cache.RemoveAsync(verifiedKey);

                return ServiceResult<UserDto>.Success(_mapper.Map<UserDto>(newUser), "Đăng ký thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for: {Email}", registerDto.Email);
                return ServiceResult<UserDto>.Failure(ApiStatusCodes.InternalServerError, "Lỗi server khi đăng ký.");
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
        public async Task<IServiceResult<AuthResultDto>> RefreshTokenAsync(string refreshToken, string accessToken)
        {
            try
            {
                var refreshTokenEntity = await _unitOfWork.RefreshTokenService.GetRefreshTokenWithAccessTokenValidationAsync(refreshToken, accessToken);

                if (refreshTokenEntity == null)
                {
                    return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.Unauthorized, "Refresh token không hợp lệ hoặc đã hết hạn.");
                }

                var user = await _unitOfWork.UserRepository.GetByIdAsync(refreshTokenEntity.UserId);
                if (user == null || user.IsActive != true)
                {
                    await _unitOfWork.RefreshTokenService.RevokeRefreshTokenAsync(refreshToken);
                    return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.Forbidden, "Tài khoản không còn hoạt động.");
                }

                var newAccessToken = _tokenService.GenerateAccessToken(user.UserId, user.Email, user.Role);
                var newRefreshTokenEntity = await _unitOfWork.RefreshTokenService.GenerateRefreshTokenAsync(user.UserId, newAccessToken);

                await _unitOfWork.RefreshTokenService.RevokeRefreshTokenAsync(refreshToken);

                var authResult = _mapper.Map<AuthResultDto>(user);
                authResult.AccessToken = newAccessToken;
                authResult.RefreshToken = newRefreshTokenEntity.Token;
                authResult.ExpiresAt = _tokenService.GetTokenExpiration();

                return ServiceResult<AuthResultDto>.Success(authResult, "Làm mới token thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return ServiceResult<AuthResultDto>.Failure(ApiStatusCodes.InternalServerError, "Lỗi khi làm mới token.");
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