using AltensorAuthService.Domain.Entities;

namespace AltensorAuthService.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(Guid id);
        Task<ApplicationUser?> GetByEmailAndTenantAsync(string email, Guid tenantId);
        Task<ApplicationUser?> GetUserWithRolesAsync(Guid userId);
        Task<List<ApplicationUser>> GetUsersByTenantAsync(Guid tenantId);
        Task<List<ApplicationUser>> GetActiveUsersByTenantAsync(Guid tenantId);
        Task<ApplicationUser> AddAsync(ApplicationUser user);
        Task UpdateAsync(ApplicationUser user);
        Task<bool> EmailExistsInTenantAsync(string email, Guid tenantId);
        Task DeactivateAsync(Guid userId);
        Task ActivateAsync(Guid userId);
        Task DeactivateAllUsersOfTenantAsync(Guid tenantId);
    }
}
