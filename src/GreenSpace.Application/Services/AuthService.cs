using AutoMapper;
using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.DTOs.User;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.External;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Constants;
using GreenSpace.Domain.Interfaces;
using GreenSpace.Domain.Models;
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
        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordService passwordHashService,
            ITokenService tokenService,
            ILogger<AuthService> logger,
            IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _passwordHashService = passwordHashService;
            _tokenService = tokenService;
            _logger = logger;
            _mapper = mapper;
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
                // 1. Validation: Check Email
                var emailExists = await _unitOfWork.UserRepository.EmailExistsAsync(registerDto.Email);
                if (emailExists)
                {
                    _logger?.LogWarning("Registration failed: Email {Email} already exists", registerDto.Email);
                    return ServiceResult<UserDto>.Failure("Email already exists");
                }

                // 2. Validation: Check Phone
                var phoneExists = await _unitOfWork.UserRepository.PhoneExistsAsync(registerDto.PhoneNumber);
                if (phoneExists)
                {
                    _logger?.LogWarning("Registration failed: Phone {Phone} already exists", registerDto.PhoneNumber);
                    return ServiceResult<UserDto>.Failure("Phone already exists");
                }

                var newUser = _mapper.Map<User>(registerDto);

                newUser.Role = Roles.Customer;

                newUser.PasswordHash = _passwordHashService.HashPassword(registerDto.Password);

                await _unitOfWork.UserRepository.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(newUser);

                _logger?.LogInformation("User registered successfully: {Email}", registerDto.Email);

                return ServiceResult<UserDto>.Success(userDto, "Registration successful");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during registration for email: {Email}", registerDto.Email); 
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return ServiceResult<UserDto>.Failure($"Error during registration: {errorMessage}");
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
                // 1. Validation: Check Email
                var emailExists = await _unitOfWork.UserRepository.EmailExistsAsync(createInternalDto.Email);
                if (emailExists)
                {
                    _logger?.LogWarning("Staff creation failed: Email {Email} already exists", createInternalDto.Email);
                    return ServiceResult<UserDto>.Failure("Email already exists");
                }

                var normalizedRole = Roles.Normalize(createInternalDto.Role);

                if (normalizedRole == null)
                {
                    _logger?.LogWarning("Staff creation failed. Invalid role: {Role}", createInternalDto.Role);
                    return ServiceResult<UserDto>.Failure($"Invalid role '{createInternalDto.Role}'. Role must be one of: {string.Join(", ", Roles.All)}");
                }

                var newUser = _mapper.Map<User>(createInternalDto);

                newUser.Role = createInternalDto.Role;
                newUser.PasswordHash = _passwordHashService.HashPassword(createInternalDto.Password);

                newUser.IsActive = true;

                await _unitOfWork.UserRepository.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                // Map Entity -> Output DTO
                var userDto = _mapper.Map<UserDto>(newUser);

                _logger?.LogInformation("Staff member created successfully: {Email} with role: {Role}", createInternalDto.Email, createInternalDto.Role);
                return ServiceResult<UserDto>.Success(userDto, "Staff member created successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during staff creation for email: {Email}", createInternalDto.Email);
                return ServiceResult<UserDto>.Failure($"Error during staff creation: {ex.Message}");
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