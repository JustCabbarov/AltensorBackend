using AltensorAuthService.Domain.Entities;

namespace AltensorAuthService.Domain.Repositories
{
    public interface IRoleRepository
    {
        Task<ApplicationRole?> GetByIdAsync(Guid id);
        Task<ApplicationRole?> GetRoleWithPermissionsAsync(Guid roleId);
        Task<List<ApplicationRole>> GetSystemRolesAsync();
        Task<List<ApplicationRole>> GetTenantRolesAsync(Guid tenantId);
        Task<List<ApplicationRole>> GetVisibleRolesForTenantAsync(Guid tenantId);
        Task<ApplicationRole> AddAsync(ApplicationRole role);
        Task UpdateAsync(ApplicationRole role);
        Task DeleteAsync(Guid roleId);
        Task<bool> RoleNameExistsInTenantAsync(string name, Guid tenantId);
        Task UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds);
    }
}
