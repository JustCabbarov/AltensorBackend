using AltensorAuthService.Domain.Entities;

namespace AltensorAuthService.Domain.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
        Task<List<RefreshToken>> GetActiveTokensByUserAsync(Guid userId);
        Task<RefreshToken> AddAsync(RefreshToken token);
        Task RevokeAsync(string tokenHash, string? reason = null);
        Task RevokeAllForUserAsync(Guid userId);
        Task RevokeAllForTenantAsync(Guid tenantId);
        Task CleanupExpiredAsync();
    }
}
