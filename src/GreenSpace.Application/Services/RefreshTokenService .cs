using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly int _refreshTokenExpiryDays;

        public RefreshTokenService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<RefreshTokenService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
            _refreshTokenExpiryDays = int.TryParse(
                _configuration["JwtSettings:RefreshTokenExpiryDays"],
                out var days) ? days : 7;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(
            Guid userId,
            string accessToken,
            string jwtId)
        {
            var accessTokenHash = TokenHashUtility.ComputeTokenHash(accessToken);

            _logger.LogDebug(
                "Generating refresh token for user {UserId} with JwtId: {JwtId}",
                userId,
                jwtId);

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = accessTokenHash,
                JwtId = jwtId,
                ExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
                AddedDate = DateTime.UtcNow,
                IsUsed = false,
                IsRevoked = false
            };

            await _unitOfWork.RefreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Refresh token created with ID: {TokenId} for user {UserId}",
                refreshToken.Id,
                userId);

            return refreshToken;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            if (!Guid.TryParse(token, out var tokenId))
            {
                _logger.LogWarning("Invalid refresh token format: {Token}", token);
                return false;
            }

            var refreshToken = await _unitOfWork.RefreshTokenRepository
                .GetByTokenIdAsync(tokenId);

            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh token not found: {TokenId}", tokenId);
                return false;
            }

            if (refreshToken.IsRevoked == true)
            {
                _logger.LogWarning("Refresh token is revoked: {TokenId}", tokenId);
                return false;
            }

            if (refreshToken.IsUsed == true)
            {
                _logger.LogWarning("Refresh token already used: {TokenId}", tokenId);
                return false;
            }

            if (refreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired: {TokenId}", tokenId);
                return false;
            }

            return true;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            if (!Guid.TryParse(token, out var tokenId))
                return null;

            return await _unitOfWork.RefreshTokenRepository
                .GetByTokenIdWithUserAsync(tokenId);
        }

        public async Task<RefreshToken?> GetRefreshTokenWithAccessTokenValidationAsync(
            string refreshToken,
            string accessToken)
        {
            if (!Guid.TryParse(refreshToken, out var tokenId))
            {
                _logger.LogWarning("Invalid refresh token format");
                return null;
            }

            var accessTokenHash = TokenHashUtility.ComputeTokenHash(accessToken);

            _logger.LogDebug(
                "Validating refresh token {TokenId} with access token hash: {Hash}",
                tokenId,
                accessTokenHash[..8] + "...");

            var storedToken = await _unitOfWork.RefreshTokenRepository
                .GetByTokenIdAndAccessTokenHashAsync(tokenId, accessTokenHash);

            if (storedToken == null)
            {
                _logger.LogWarning(
                    "Refresh token validation failed - token not found or access token mismatch");
                return null;
            }

            // Additional validation
            if (storedToken.IsRevoked == true ||
                storedToken.IsUsed == true ||
                storedToken.ExpiryDate <= DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "Refresh token {TokenId} failed validation - Revoked: {IsRevoked}, Used: {IsUsed}, Expired: {IsExpired}",
                    tokenId,
                    storedToken.IsRevoked,
                    storedToken.IsUsed,
                    storedToken.ExpiryDate <= DateTime.UtcNow);
                return null;
            }

            return storedToken;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            if (!Guid.TryParse(token, out var tokenId))
                return false;

            var refreshToken = await _unitOfWork.RefreshTokenRepository
                .GetByIdAsync(tokenId);

            if (refreshToken == null)
            {
                _logger.LogWarning("Attempted to revoke non-existent token: {TokenId}", tokenId);
                return false;
            }

            refreshToken.IsRevoked = true;
            await _unitOfWork.RefreshTokenRepository.UpdateAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked: {TokenId}", tokenId);
            return true;
        }

        public async Task<bool> RevokeAllUserRefreshTokensAsync(Guid userId)
        {
            var userTokens = await _unitOfWork.RefreshTokenRepository
                .GetByUserIdAsync(userId);

            if (!userTokens.Any())
            {
                _logger.LogInformation("No tokens found to revoke for user: {UserId}", userId);
                return false;
            }

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
                await _unitOfWork.RefreshTokenRepository.UpdateAsync(token);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Revoked {Count} refresh tokens for user: {UserId}",
                userTokens.Count(),
                userId);

            return true;
        }

        public async Task<bool> IsRefreshTokenActiveAsync(string token)
        {
            return await ValidateRefreshTokenAsync(token);
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _unitOfWork.RefreshTokenRepository
                .GetExpiredTokensAsync();

            if (expiredTokens.Any())
            {
                foreach (var token in expiredTokens)
                {
                    await _unitOfWork.RefreshTokenRepository.RemoveAsync(token);
                }
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Cleaned up {Count} expired refresh tokens",
                    expiredTokens.Count());
            }
        }
    }
}