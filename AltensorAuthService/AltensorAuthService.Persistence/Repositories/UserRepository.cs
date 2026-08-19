using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Repositories;
using AltensorAuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AltensorAuthService.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task ActivateAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.IsActive = true;
            }
        }

        public async Task<ApplicationUser> AddAsync(ApplicationUser user)
        {
            await _context.Users.AddAsync(user);
            return user;
        }

        public async Task DeactivateAllUsersOfTenantAsync(Guid tenantId)
        {
            var users = await _context.Users
                .Where(u => u.TenantId == tenantId)
                .ToListAsync();

            foreach (var user in users)
            {
                user.IsActive = false;
            }
        }

        public async Task DeactivateAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.IsActive = false;
            }
        }

        public async Task<bool> EmailExistsInTenantAsync(string email, Guid tenantId)
        {
            var normalizedEmail = email.Trim().ToUpperInvariant();
            return await _context.Users
                .AnyAsync(u => u.NormalizedEmail == normalizedEmail && u.TenantId == tenantId);
        }

        public async Task<List<ApplicationUser>> GetActiveUsersByTenantAsync(Guid tenantId)
        {
            return await _context.Users
                .Where(u => u.TenantId == tenantId && u.IsActive)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetByEmailAndTenantAsync(string email, Guid tenantId)
        {
            var normalizedEmail = email.Trim().ToUpperInvariant();
            return await _context.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.TenantId == tenantId);
        }

        public async Task<ApplicationUser?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<ApplicationUser>> GetUsersByTenantAsync(Guid tenantId)
        {
            return await _context.Users
                .Where(u => u.TenantId == tenantId)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserWithRolesAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }
    }
}
