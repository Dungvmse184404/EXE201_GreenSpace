using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.DTOs.User;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IAuthService
    {

        Task<IServiceResult<AuthResultDto>> LoginAsync(LoginDto loginDto);
        Task<IServiceResult<UserDto>> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// Create a new staff member (Admin only)
        /// </summary>
        /// <param name="internalUserDto">Staff creation data</param>
        /// <returns>Authentication result</returns>
        Task<IServiceResult<UserDto>> CreateInternalUserAsync(InternalUserDto internalUserDto);

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="accessToken">Current access token to validate</param>
        /// <returns>New authentication result</returns>
        Task<IServiceResult<AuthResultDto>> RefreshTokenAsync(string refreshToken, string accessToken);

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token to revoke</param>
        /// <returns>Success status</returns>
        Task<IServiceResult<bool>> RevokeTokenAsync(string refreshToken);

        /// <summary>
        /// Revoke all refresh tokens for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success status</returns>
        Task<IServiceResult<bool>> RevokeAllUserTokensAsync(Guid userId);



        Task<IServiceResult> RegisterMailAsync(RegisteMailDto dto);

        Task<IServiceResult> VerifyRegisterOtpAsync(VerifyRegisterOtpDto dto);

        // =================================================================
        // RESET PASSWORD
        // =================================================================

        /// <summary>
        /// Khởi tạo reset password - kiểm tra email tồn tại và gửi OTP
        /// </summary>
        /// <param name="dto">Email cần reset password</param>
        /// <returns>Success nếu email hợp lệ và OTP đã gửi</returns>
        Task<IServiceResult> ForgotPasswordAsync(ForgotPasswordDto dto);

        /// <summary>
        /// Xác thực OTP reset password
        /// </summary>
        /// <param name="dto">Email và OTP</param>
        /// <returns>Success nếu OTP hợp lệ</returns>
        Task<IServiceResult> VerifyResetPasswordOtpAsync(VerifyResetPasswordOtpDto dto);

        /// <summary>
        /// Đặt mật khẩu mới sau khi đã verify OTP
        /// </summary>
        /// <param name="dto">Email và mật khẩu mới</param>
        /// <returns>Success nếu đổi mật khẩu thành công</returns>
        Task<IServiceResult> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
