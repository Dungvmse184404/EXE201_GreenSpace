using GreenSpace.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Interfaces.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string accessToken);
        Task<bool> ValidateRefreshTokenAsync(string token);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<RefreshToken?> GetRefreshTokenWithAccessTokenValidationAsync(string token, string accessToken);
        Task<bool> RevokeRefreshTokenAsync(string token);
        Task<bool> RevokeAllUserRefreshTokensAsync(Guid userId);
        Task<bool> IsRefreshTokenActiveAsync(string token);
        Task<bool> ValidateAccessTokenHashAsync(Guid userId, string accessTokenHash);
        Task CleanupExpiredTokensAsync();
    }
}
