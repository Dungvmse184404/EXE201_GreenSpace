using System.Security.Claims;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Generate JWT access token
        /// </summary>
        string GenerateAccessToken(Guid userId, string email, string role, string jti);

        /// <summary>
        /// Generate cryptographically secure refresh token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Get principal from expired token (for refresh flow)
        /// </summary>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        /// <summary>
        /// Validate token signature and structure (ignore expiration)
        /// </summary>
        bool ValidateToken(string token);

        /// <summary>
        /// Get token expiration time
        /// </summary>
        DateTime GetTokenExpiration();

        /// <summary>
        /// Extract JwtId (jti claim) from token
        /// </summary>
        string? GetJwtIdFromToken(string token);
    }
}