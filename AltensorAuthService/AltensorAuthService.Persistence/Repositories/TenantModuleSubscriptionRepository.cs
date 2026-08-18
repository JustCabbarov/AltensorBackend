using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Enums;
using AltensorAuthService.Domain.Repositories;
using AltensorAuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AltensorAuthService.Persistence.Repositories
{
    public class TenantModuleSubscriptionRepository : ITenantModuleSubscriptionRepository
    {
        private readonly IGenericRepository<TenantModuleSubscription> _generic;
        private readonly AppDbContext _context;

        public TenantModuleSubscriptionRepository(IGenericRepository<TenantModuleSubscription> generic, AppDbContext context)
        {
            _generic = generic;
            _context = context;
        }

        public async Task ActivateAllModulesAsync(Guid tenantId)
        {
            var subs = await _context.TenantModuleSubscriptions
                .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .ToListAsync();

            foreach (var sub in subs)
            {
                sub.Status = SubscriptionStatus.Active;
                sub.SuspendedAt = null;
                sub.SuspendReason = null;
                sub.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task ActivateAsync(Guid tenantId, Guid moduleId)
        {
            var sub = await _context.TenantModuleSubscriptions
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ModuleId == moduleId && !s.IsDeleted);

            if (sub != null)
            {
                sub.Status = SubscriptionStatus.Active;
                sub.SuspendedAt = null;
                sub.SuspendReason = null;
                sub.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<TenantModuleSubscription> AddAsync(TenantModuleSubscription sub)
        {
            return await _generic.AddAsync(sub);
        }

        public async Task DeleteAsync(Guid tenantId, Guid moduleId)
        {
            var sub = await _context.TenantModuleSubscriptions
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ModuleId == moduleId && !s.IsDeleted);

            if (sub != null)
            {
                sub.IsDeleted = true;
                sub.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task<List<string>> GetActiveModuleCodesAsync(Guid tenantId)
        {
            return await _context.TenantModuleSubscriptions
                .Where(s => s.TenantId == tenantId 
                         && s.Status == SubscriptionStatus.Active 
                         && !s.IsDeleted
                         && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
                .Select(s => s.Module.Code)
                .ToListAsync();
        }

        public async Task<List<TenantModuleSubscription>> GetActiveSubscriptionsAsync(Guid tenantId)
        {
            return await _context.TenantModuleSubscriptions
                .Include(s => s.Module)
                .Where(s => s.TenantId == tenantId 
                         && s.Status == SubscriptionStatus.Active 
                         && !s.IsDeleted
                         && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task<List<TenantModuleSubscription>> GetAllByTenantAsync(Guid tenantId)
        {
            return await _context.TenantModuleSubscriptions
                .Include(s => s.Module)
                .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .ToListAsync();
        }

        public async Task<TenantModuleSubscription?> GetSubscriptionAsync(Guid tenantId, Guid moduleId)
        {
            return await _context.TenantModuleSubscriptions
                .Include(s => s.Module)
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ModuleId == moduleId && !s.IsDeleted);
        }

        public async Task SuspendAllModulesAsync(Guid tenantId, string reason)
        {
            var subs = await _context.TenantModuleSubscriptions
                .Where(s => s.TenantId == tenantId && !s.IsDeleted)
                .ToListAsync();

            foreach (var sub in subs)
            {
                sub.Status = SubscriptionStatus.Suspended;
                sub.SuspendedAt = DateTime.UtcNow;
                sub.SuspendReason = reason;
                sub.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task SuspendAsync(Guid tenantId, Guid moduleId, string reason)
        {
            var sub = await _context.TenantModuleSubscriptions
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ModuleId == moduleId && !s.IsDeleted);

            if (sub != null)
            {
                sub.Status = SubscriptionStatus.Suspended;
                sub.SuspendedAt = DateTime.UtcNow;
                sub.SuspendReason = reason;
                sub.UpdatedAt = DateTime.UtcNow;
            }
        }

        public async Task UpdateAsync(TenantModuleSubscription sub)
        {
            await _generic.UpdateAsync(sub);
        }
    }
}
