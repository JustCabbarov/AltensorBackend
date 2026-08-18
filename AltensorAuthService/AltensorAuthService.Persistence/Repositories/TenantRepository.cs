using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Enums;
using AltensorAuthService.Domain.Repositories;
using AltensorAuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AltensorAuthService.Persistence.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly IGenericRepository<Tenant> _generic;
        private readonly AppDbContext _context;

        public TenantRepository(IGenericRepository<Tenant> generic, AppDbContext context)
        {
            _generic = generic;
            _context = context;
        }

        public async Task<Tenant> AddAsync(Tenant tenant)
        {
            return await _generic.AddAsync(tenant);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Tenants.AnyAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task<List<Tenant>> GetAllAsync()
        {
            return await _generic.GetAllAsync();
        }

        public async Task<Tenant?> GetByIdAsync(Guid id)
        {
            return await _generic.GetByIdAsync(id);
        }

        public async Task<Tenant?> GetBySlugAsync(string slug)
        {
            var normalizedSlug = slug.Trim().ToLower();
            return await _context.Tenants
                .FirstOrDefaultAsync(t => t.Slug.ToLower() == normalizedSlug && !t.IsDeleted);
        }

        public async Task<List<Tenant>> GetByStatusAsync(TenantStatus status)
        {
            return await _context.Tenants
                .Where(t => t.Status == status && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            var normalizedSlug = slug.Trim().ToLower();
            return await _context.Tenants
                .AnyAsync(t => t.Slug.ToLower() == normalizedSlug && !t.IsDeleted);
        }

        public async Task UpdateAsync(Tenant tenant)
        {
            await _generic.UpdateAsync(tenant);
        }
    }
}
