using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Generate and save refresh token to database
        /// </summary>
        Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string accessToken, string jwtId);

        /// <summary>
        /// Validate refresh token (check expiry, revoked, used)
        /// </summary>
        Task<bool> ValidateRefreshTokenAsync(string token);

        /// <summary>
        /// Get refresh token with user info
        /// </summary>
        Task<RefreshToken?> GetRefreshTokenAsync(string token);

        /// <summary>
        /// Get refresh token with access token validation
        /// </summary>
        Task<RefreshToken?> GetRefreshTokenWithAccessTokenValidationAsync(string refreshToken, string accessToken);

        /// <summary>
        /// Revoke a specific refresh token
        /// </summary>
        Task<bool> RevokeRefreshTokenAsync(string token);

        /// <summary>
        /// Revoke all user's refresh tokens
        /// </summary>
        Task<bool> RevokeAllUserRefreshTokensAsync(Guid userId);

        /// <summary>
        /// Check if refresh token is active
        /// </summary>
        Task<bool> IsRefreshTokenActiveAsync(string token);

        /// <summary>
        /// Cleanup expired tokens
        /// </summary>
        Task CleanupExpiredTokensAsync();
    }
}