using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Enums;

namespace AltensorAuthService.Domain.Repositories
{
    public interface ITenantModuleSubscriptionRepository
    {
        Task<List<TenantModuleSubscription>> GetActiveSubscriptionsAsync(Guid tenantId);
        Task<List<string>> GetActiveModuleCodesAsync(Guid tenantId);
        Task<TenantModuleSubscription?> GetSubscriptionAsync(Guid tenantId, Guid moduleId);
        Task<List<TenantModuleSubscription>> GetAllByTenantAsync(Guid tenantId);
        Task<TenantModuleSubscription> AddAsync(TenantModuleSubscription sub);
        Task UpdateAsync(TenantModuleSubscription sub);
        Task SuspendAsync(Guid tenantId, Guid moduleId, string reason);
        Task ActivateAsync(Guid tenantId, Guid moduleId);
        Task SuspendAllModulesAsync(Guid tenantId, string reason);
        Task ActivateAllModulesAsync(Guid tenantId);
        Task DeleteAsync(Guid tenantId, Guid moduleId);
    }
}
