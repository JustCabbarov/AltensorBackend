using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Repositories;
using AltensorAuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AltensorAuthService.Persistence.Repositories
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly IGenericRepository<SistemModule> _generic;
        private readonly AppDbContext _context;

        public ModuleRepository(IGenericRepository<SistemModule> generic, AppDbContext context)
        {
            _generic = generic;
            _context = context;
        }

        public async Task<SistemModule> AddAsync(SistemModule module)
        {
            return await _generic.AddAsync(module);
        }

        public async Task<List<SistemModule>> GetAllActiveAsync()
        {
            return await _context.Modules
                .Include(m => m.Permissions)
                .Where(m => m.IsActive && !m.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<SistemModule>> GetAllAsync()
        {
            return await _context.Modules
                .Include(m => m.Permissions)
                .Where(m => !m.IsDeleted)
                .ToListAsync();
        }

        public async Task<SistemModule?> GetByCodeAsync(string code)
        {
            var normalized = code.Trim().ToLower();
            return await _context.Modules
                .Include(m => m.Permissions)
                .FirstOrDefaultAsync(m => m.Code.ToLower() == normalized && !m.IsDeleted);
        }

        public async Task<SistemModule?> GetByIdAsync(Guid id)
        {
            return await _context.Modules
                .Include(m => m.Permissions)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<List<SistemModule>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            return await _context.Modules
                .Include(m => m.Permissions)
                .Where(m => idList.Contains(m.Id) && !m.IsDeleted)
                .ToListAsync();
        }
    }
}
