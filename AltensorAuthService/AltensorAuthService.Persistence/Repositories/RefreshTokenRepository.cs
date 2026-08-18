using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Repositories;
using AltensorAuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AltensorAuthService.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IGenericRepository<RefreshToken> _generic;
        private readonly AppDbContext _context;

        public RefreshTokenRepository(IGenericRepository<RefreshToken> generic, AppDbContext context)
        {
            _generic = generic;
            _context = context;
        }

        public async Task<RefreshToken> AddAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
            return token;
        }

        public async Task CleanupExpiredAsync()
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var expiredTokens = await _context.RefreshTokens
                .Where(t => t.ExpiresAt < cutoff || (t.Revoked && t.RevokedAt < cutoff))
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.Revoked && t.ExpiresAt > DateTime.UtcNow && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(t => t.User)
                    .ThenInclude(u => u.Tenant)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsDeleted);
        }

        public async Task RevokeAllForTenantAsync(Guid tenantId)
        {
            var tokens = await _context.RefreshTokens
                .Include(t => t.User)
                .Where(t => t.User.TenantId == tenantId && !t.Revoked && !t.IsDeleted)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task RevokeAllForUserAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.Revoked && !t.IsDeleted)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task RevokeAsync(string tokenHash, string? reason = null)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsDeleted);

            if (token != null)
            {
                token.Revoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
