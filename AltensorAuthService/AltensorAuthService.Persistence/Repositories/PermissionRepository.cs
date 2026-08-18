using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Repositories;
using AltensorAuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AltensorAuthService.Persistence.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly IGenericRepository<Permission> _generic;
        private readonly AppDbContext _context;

        public PermissionRepository(IGenericRepository<Permission> generic, AppDbContext context)
        {
            _generic = generic;
            _context = context;
        }

        public async Task<List<Permission>> GetAllAsync()
        {
            return await _context.Permissions
                .Include(p => p.Module)
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<Permission?> GetByCodeAsync(string code)
        {
            var normalizedCode = code.Trim().ToLower();
            return await _context.Permissions
                .Include(p => p.Module)
                .FirstOrDefaultAsync(p => p.Code.ToLower() == normalizedCode && !p.IsDeleted);
        }

        public async Task<List<Permission>> GetByCodesAsync(IEnumerable<string> codes)
        {
            var normalizedCodes = codes.Select(c => c.Trim().ToLower()).ToList();
            return await _context.Permissions
                .Include(p => p.Module)
                .Where(p => normalizedCodes.Contains(p.Code.ToLower()) && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(Guid id)
        {
            return await _context.Permissions
                .Include(p => p.Module)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<List<Permission>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            return await _context.Permissions
                .Include(p => p.Module)
                .Where(p => idList.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<Permission>> GetByModuleAsync(Guid moduleId)
        {
            return await _context.Permissions
                .Include(p => p.Module)
                .Where(p => p.ModuleId == moduleId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<Permission>> GetByModuleCodeAsync(string moduleCode)
        {
            var normalized = moduleCode.Trim().ToLower();
            return await _context.Permissions
                .Include(p => p.Module)
                .Where(p => p.Module.Code.ToLower() == normalized && !p.IsDeleted)
                .ToListAsync();
        }
    }
}
