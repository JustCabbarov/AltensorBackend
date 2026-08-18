using AltensorAuthService.Application.Exceptions;
using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Contract.Permissions;
using AltensorAuthService.Contract.Roles;
using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AltensorAuthService.Application.Services
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RoleManagementService> _logger;

        public RoleManagementService(
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            ICurrentTenantService currentTenantService,
            IUnitOfWork unitOfWork,
            ILogger<RoleManagementService> logger)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _currentTenantService = currentTenantService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        private Guid GetRequiredTenantId()
        {
            if (!_currentTenantService.TenantId.HasValue)
            {
                _logger.LogWarning("Tenant konteksti tapılmadı və ya istifadəçi daxil olmayıb.");
                throw new UnauthorizedException("Tenant konteksti tapılmadı.");
            }
            return _currentTenantService.TenantId.Value;
        }

        public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("Yeni rol yaratma prosesi başladı: RoleName='{RoleName}', TenantId={TenantId}", request.Name, tenantId);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("Rol adı boş ola bilməz.");
                throw new ValidationException("Rol adı tələb olunur.");
            }

            var nameExists = await _roleRepository.RoleNameExistsInTenantAsync(request.Name, tenantId);
            if (nameExists)
            {
                _logger.LogWarning("Rol artıq mövcuddur: RoleName='{RoleName}', TenantId={TenantId}", request.Name, tenantId);
                throw new ValidationException($"'{request.Name}' adlı rol artıq mövcuddur.");
            }

            var role = new ApplicationRole
            {
                Name = request.Name.Trim(),
                NormalizedName = request.Name.Trim().ToUpper(),
                Description = request.Description?.Trim(),
                TenantId = tenantId,
                IsSystemRole = false
            };

            await _roleRepository.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            if (request.PermissionIds != null && request.PermissionIds.Any())
            {
                var validPermissions = await _permissionRepository.GetByIdsAsync(request.PermissionIds);
                await _roleRepository.UpdateRolePermissionsAsync(role.Id, validPermissions.Select(p => p.Id));
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Rola icazələr təyin edildi: RoleId={RoleId}, İcazə sayı={PermCount}", role.Id, validPermissions.Count);
            }

            var createdRole = await _roleRepository.GetRoleWithPermissionsAsync(role.Id);
            _logger.LogInformation("Rol uğurla yaradıldı: RoleId={RoleId}, RoleName='{RoleName}', TenantId={TenantId}", role.Id, role.Name, tenantId);
            return MapToRoleResponse(createdRole!);
        }

        public async Task DeleteRoleAsync(Guid roleId)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("Rol silinməsi sorğusu: RoleId={RoleId}, TenantId={TenantId}", roleId, tenantId);

            var role = await _roleRepository.GetByIdAsync(roleId);

            if (role == null || role.TenantId != tenantId)
            {
                _logger.LogWarning("Silinəcək rol tapılmadı və ya başqa tenanta aiddir: RoleId={RoleId}, TenantId={TenantId}", roleId, tenantId);
                throw new NotFoundException("Rol tapılmadı.");
            }

            if (role.IsSystemRole)
            {
                _logger.LogWarning("Sistem rolu silinə bilməz: RoleId={RoleId}, RoleName='{RoleName}'", role.Id, role.Name);
                throw new ForbiddenException("Sistem rolları silinə bilməz.");
            }

            await _roleRepository.DeleteAsync(roleId);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Rol uğurla silindi: RoleId={RoleId}, TenantId={TenantId}", roleId, tenantId);
        }

        public async Task<List<PermissionResponse>> GetAllPermissionsAsync(string? moduleCode = null)
        {
            _logger.LogInformation("Bütün icazələr sorğulanır: ModuleFilter='{ModuleCode}'", moduleCode ?? "Hamısı");

            var permissions = string.IsNullOrWhiteSpace(moduleCode)
                ? await _permissionRepository.GetAllAsync()
                : await _permissionRepository.GetByModuleCodeAsync(moduleCode);

            _logger.LogInformation("İcazələr gətirildi: Say={Count}", permissions.Count);

            return permissions.Select(p => new PermissionResponse
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                ModuleId = p.ModuleId,
                ModuleCode = p.Module?.Code ?? string.Empty
            }).ToList();
        }

        public async Task<RoleResponse> GetRoleByIdAsync(Guid roleId)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("Rol məlumatları sorğulanır: RoleId={RoleId}, TenantId={TenantId}", roleId, tenantId);

            var role = await _roleRepository.GetRoleWithPermissionsAsync(roleId);

            if (role == null || (role.TenantId != null && role.TenantId != tenantId))
            {
                _logger.LogWarning("Rol tapılmadı: RoleId={RoleId}, TenantId={TenantId}", roleId, tenantId);
                throw new NotFoundException("Rol tapılmadı.");
            }

            return MapToRoleResponse(role);
        }

        public async Task<List<RoleResponse>> GetVisibleRolesAsync()
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("Tenant üçün görünən rollar sorğulanır: TenantId={TenantId}", tenantId);

            var roles = await _roleRepository.GetVisibleRolesForTenantAsync(tenantId);
            _logger.LogInformation("Tenant üçün {Count} ədəd rol tapıldı: TenantId={TenantId}", roles.Count, tenantId);

            return roles.Select(MapToRoleResponse).ToList();
        }

        public async Task<RoleResponse> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request)
        {
            var tenantId = GetRequiredTenantId();
            _logger.LogInformation("Rol redaktəsi başladı: RoleId={RoleId}, TenantId={TenantId}", roleId, tenantId);

            var role = await _roleRepository.GetByIdAsync(roleId);

            if (role == null || role.TenantId != tenantId)
            {
                _logger.LogWarning("Redaktə olunacaq rol tapılmadı: RoleId={RoleId}, TenantId={TenantId}", roleId, tenantId);
                throw new NotFoundException("Rol tapılmadı.");
            }

            if (role.IsSystemRole)
            {
                _logger.LogWarning("Sistem rolu redaktə edilə bilməz: RoleId={RoleId}", roleId);
                throw new ForbiddenException("Sistem rolları redaktə edilə bilməz.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                role.Name = request.Name.Trim();
                role.NormalizedName = request.Name.Trim().ToUpper();
            }

            role.Description = request.Description?.Trim();

            await _roleRepository.UpdateAsync(role);

            if (request.PermissionIds != null)
            {
                var validPermissions = await _permissionRepository.GetByIdsAsync(request.PermissionIds);
                await _roleRepository.UpdateRolePermissionsAsync(role.Id, validPermissions.Select(p => p.Id));
                _logger.LogInformation("Rol icazələri yeniləndi: RoleId={RoleId}, İcazə sayı={Count}", role.Id, validPermissions.Count);
            }

            await _unitOfWork.SaveChangesAsync();

            var updated = await _roleRepository.GetRoleWithPermissionsAsync(roleId);
            _logger.LogInformation("Rol uğurla yeniləndi: RoleId={RoleId}, RoleName='{RoleName}'", role.Id, role.Name);

            return MapToRoleResponse(updated!);
        }

        private static RoleResponse MapToRoleResponse(ApplicationRole role)
        {
            var permissions = role.RolePermissions?
                .Where(rp => rp.Permission != null)
                .Select(rp => rp.Permission.Code)
                .ToList() ?? new List<string>();

            var permissionIds = role.RolePermissions?
                .Select(rp => rp.PermissionId)
                .ToList() ?? new List<Guid>();

            return new RoleResponse
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                TenantId = role.TenantId,
                IsSystemRole = role.IsSystemRole,
                Permissions = permissions,
                PermissionIds = permissionIds
            };
        }
    }
}
