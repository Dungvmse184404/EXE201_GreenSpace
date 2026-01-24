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

        public RefreshTokenService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<RefreshTokenService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
            _refreshTokenExpiryDays = int.TryParse(_configuration["JwtSettings:RefreshTokenExpiryDays"], out var days) ? days : 7;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string accessToken)
        {
            var accessTokenHash = TokenHashUtility.ComputeTokenHash(accessToken);
            _logger.LogDebug("Generating refresh token for user {UserId} with access token hash: {Hash}",
                userId, accessTokenHash[..8] + "..."); // Log first 8 chars for debugging

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = accessTokenHash,
                ExpiryDate = DateTime.Now.AddDays(_refreshTokenExpiryDays),
                AddedDate = DateTime.Now
            };

            await _unitOfWork.RefreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogDebug("Refresh token created with ID: {TokenId}", refreshToken.Id);
            return refreshToken;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            if (!Guid.TryParse(token, out var tokenId))
                return false;

            var refreshToken = await _unitOfWork.RefreshTokenRepository.GetByTokenIdAsync(tokenId);
            return refreshToken != null;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            if (!Guid.TryParse(token, out var tokenId))
                return null;

            return await _unitOfWork.RefreshTokenRepository.GetByTokenIdWithUserAsync(tokenId);
        }

        public async Task<RefreshToken?> GetRefreshTokenWithAccessTokenValidationAsync(string token, string accessToken)
        {
            if (!Guid.TryParse(token, out var tokenId))
                return null;

            var accessTokenHash = TokenHashUtility.ComputeTokenHash(accessToken);

            return await _unitOfWork.RefreshTokenRepository.GetByTokenIdAndAccessTokenHashAsync(tokenId, accessTokenHash);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            if (!Guid.TryParse(token, out var tokenId))
                return false;

            var refreshToken = await _unitOfWork.RefreshTokenRepository.GetByIdAsync(tokenId);

            if (refreshToken == null)
                return false;

            await _unitOfWork.RefreshTokenRepository.RemoveAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RevokeAllUserRefreshTokensAsync(Guid userId)
        {
            var userTokens = await _unitOfWork.RefreshTokenRepository.GetByUserIdAsync(userId);

            if (!userTokens.Any())
                return false;

            foreach (var token in userTokens)
            {
                await _unitOfWork.RefreshTokenRepository.RemoveAsync(token);
            }
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsRefreshTokenActiveAsync(string token)
        {
            return await ValidateRefreshTokenAsync(token);
        }

        public async Task<bool> ValidateAccessTokenHashAsync(Guid userId, string accessTokenHash)
        {
            _logger.LogDebug("Validating access token hash for user {UserId}: {Hash}",
                userId, accessTokenHash[..8] + "..."); // Log first 8 chars for debugging

            var result = await _unitOfWork.RefreshTokenRepository.ValidateAccessTokenHashAsync(userId, accessTokenHash);

            _logger.LogDebug("Access token hash validation result for user {UserId}: {Result}", userId, result);
            return result;
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _unitOfWork.RefreshTokenRepository.GetExpiredTokensAsync();

            if (expiredTokens.Any())
            {
                foreach (var token in expiredTokens)
                {
                    await _unitOfWork.RefreshTokenRepository.RemoveAsync(token);
                }
                await _unitOfWork.SaveChangesAsync();
            }
        }


    }
}
