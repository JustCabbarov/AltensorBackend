using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Repositories;
using AltensorAuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AltensorAuthService.Persistence.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationRole> AddAsync(ApplicationRole role)
        {
            await _context.Roles.AddAsync(role);
            return role;
        }

        public async Task DeleteAsync(Guid roleId)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID '{roleId}' not found.");

            if (role.IsSystemRole)
                throw new InvalidOperationException("System roles cannot be deleted.");

            // Remove associated RolePermissions
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(rolePermissions);
            _context.Roles.Remove(role);
        }

        public async Task<ApplicationRole?> GetByIdAsync(Guid id)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<ApplicationRole?> GetRoleWithPermissionsAsync(Guid roleId)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                        .ThenInclude(p => p.Module)
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task<List<ApplicationRole>> GetSystemRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.TenantId == null && r.IsSystemRole)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<List<ApplicationRole>> GetTenantRolesAsync(Guid tenantId)
        {
            return await _context.Roles
                .Where(r => r.TenantId == tenantId)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<List<ApplicationRole>> GetVisibleRolesForTenantAsync(Guid tenantId)
        {
            return await _context.Roles
                .Where(r => (r.TenantId == null && r.IsSystemRole) || r.TenantId == tenantId)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<bool> RoleNameExistsInTenantAsync(string name, Guid tenantId)
        {
            var normalizedName = name.Trim().ToUpper();
            return await _context.Roles
                .AnyAsync(r => r.NormalizedName == normalizedName && (r.TenantId == tenantId || r.TenantId == null));
        }

        public Task UpdateAsync(ApplicationRole role)
        {
            _context.Roles.Update(role);
            return Task.CompletedTask;
        }

        public async Task UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds)
        {
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(existingPermissions);

            var newRolePermissions = permissionIds.Select(pid => new RolePermission
            {
                RoleId = roleId,
                PermissionId = pid
            });

            await _context.RolePermissions.AddRangeAsync(newRolePermissions);
        }
    }
}
