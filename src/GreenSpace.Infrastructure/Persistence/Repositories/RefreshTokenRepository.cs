using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Domain.Models;
using GreenSpace.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get refresh token by token ID
        /// </summary>
        /// <param name="tokenId">Token ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refresh token if found and not expired</returns>
        public async Task<RefreshToken?> GetByTokenIdAsync(Guid tokenId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Id == tokenId && rt.ExpiryDate > DateTime.Now, cancellationToken);
        }

        /// <summary>
        /// Get refresh token with user by token ID
        /// </summary>
        /// <param name="tokenId">Token ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refresh token with user if found and not expired</returns>
        public async Task<RefreshToken?> GetByTokenIdWithUserAsync(Guid tokenId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Id == tokenId && rt.ExpiryDate > DateTime.Now, cancellationToken);
        }

        /// <summary>
        /// Get refresh token by token ID and access token hash
        /// </summary>
        /// <param name="tokenId">Token ID</param>
        /// <param name="accessTokenHash">Access token hash</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refresh token with user if found and not expired</returns>
        public async Task<RefreshToken?> GetByTokenIdAndAccessTokenHashAsync(Guid tokenId, string accessTokenHash, CancellationToken cancellationToken = default)
        {
            return await _dbSet.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Id == tokenId &&
                                   rt.Token == accessTokenHash &&
                                   rt.ExpiryDate > DateTime.Now, cancellationToken);
        }

        /// <summary>
        /// Get all refresh tokens for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of user's refresh tokens</returns>
        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Check if access token hash is valid for user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="accessTokenHash">Access token hash</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if valid</returns>
        public async Task<bool> ValidateAccessTokenHashAsync(Guid userId, string accessTokenHash, CancellationToken cancellationToken = default)
        {
            return await _dbSet.RefreshTokens
                .AnyAsync(rt => rt.UserId == userId &&
                               rt.Token == accessTokenHash &&
                               rt.ExpiryDate > DateTime.Now, cancellationToken);
        }

        /// <summary>
        /// Get expired refresh tokens
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of expired tokens</returns>
        public async Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.RefreshTokens
                .Where(rt => rt.ExpiryDate <= DateTime.Now)
                .ToListAsync(cancellationToken);
        }
    }
}
