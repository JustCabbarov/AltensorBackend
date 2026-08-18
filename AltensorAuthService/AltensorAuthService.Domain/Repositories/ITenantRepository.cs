using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Enums;

namespace AltensorAuthService.Domain.Repositories
{
    public interface ITenantRepository
    {
        Task<Tenant?> GetByIdAsync(Guid id);
        Task<Tenant?> GetBySlugAsync(string slug);
        Task<List<Tenant>> GetAllAsync();
        Task<List<Tenant>> GetByStatusAsync(TenantStatus status);
        Task<Tenant> AddAsync(Tenant tenant);
        Task UpdateAsync(Tenant tenant);
        Task<bool> SlugExistsAsync(string slug);
        Task<bool> ExistsAsync(Guid id);
    }
}
