using GreenSpace.Application.DTOs.Auth;
using GreenSpace.Application.DTOs.User;
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

    }
}
