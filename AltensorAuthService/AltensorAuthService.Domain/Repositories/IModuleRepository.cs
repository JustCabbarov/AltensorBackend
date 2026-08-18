using AltensorAuthService.Domain.Entities;

namespace AltensorAuthService.Domain.Repositories
{
    public interface IModuleRepository
    {
        Task<List<SistemModule>> GetAllActiveAsync();
        Task<List<SistemModule>> GetAllAsync();
        Task<SistemModule?> GetByCodeAsync(string code);
        Task<SistemModule?> GetByIdAsync(Guid id);
        Task<List<SistemModule>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<SistemModule> AddAsync(SistemModule module);
    }
}
