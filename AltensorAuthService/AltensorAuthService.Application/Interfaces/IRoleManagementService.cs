using AltensorAuthService.Contract.Permissions;
using AltensorAuthService.Contract.Roles;

namespace AltensorAuthService.Application.Interfaces
{
    public interface IRoleManagementService
    {
        Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request);
        Task<List<RoleResponse>> GetVisibleRolesAsync();
        Task<RoleResponse> GetRoleByIdAsync(Guid roleId);
        Task<RoleResponse> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request);
        Task DeleteRoleAsync(Guid roleId);
        Task<List<PermissionResponse>> GetAllPermissionsAsync(string? moduleCode = null);
    }
}
