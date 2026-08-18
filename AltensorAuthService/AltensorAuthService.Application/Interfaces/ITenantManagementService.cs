using AltensorAuthService.Contract.Tenants;
using AltensorAuthService.Domain.Enums;

namespace AltensorAuthService.Application.Interfaces
{
    public interface ITenantManagementService
    {
        Task<TenantResponse> CreateTenantWithAdminAsync(CreateTenantRequest request);
        Task<List<TenantResponse>> GetAllTenantsAsync(TenantStatus? status = null);
        Task<TenantDetailResponse> GetTenantDetailAsync(Guid tenantId);
        
        // Manual tenant status suspension & activation (Super Admin)
        Task SuspendTenantAsync(Guid tenantId, string? reason);
        Task UnsuspendTenantAsync(Guid tenantId);

        // Module subscription management
        Task AddModuleSubscriptionAsync(Guid tenantId, ModuleSubscriptionRequest request);
        Task RemoveModuleSubscriptionAsync(Guid tenantId, Guid moduleId);
        Task SuspendModuleSubscriptionAsync(Guid tenantId, Guid moduleId, string? reason);
        Task UnsuspendModuleSubscriptionAsync(Guid tenantId, Guid moduleId);
    }
}
