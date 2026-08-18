using AltensorAuthService.Domain.Entities;

namespace AltensorAuthService.Domain.Repositories
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetAllAsync();
        Task<List<Permission>> GetByModuleAsync(Guid moduleId);
        Task<List<Permission>> GetByModuleCodeAsync(string moduleCode);
        Task<List<Permission>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<List<Permission>> GetByCodesAsync(IEnumerable<string> codes);
        Task<Permission?> GetByCodeAsync(string code);
        Task<Permission?> GetByIdAsync(Guid id);
    }
}
