using AltensorAuthService.Contract.Users;

namespace AltensorAuthService.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);
        Task<List<UserResponse>> GetTenantUsersAsync();
        Task<UserResponse> GetUserByIdAsync(Guid userId);
        Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request);
        Task DeactivateUserAsync(Guid userId);
        Task ActivateUserAsync(Guid userId);
        Task AssignRoleAsync(Guid userId, Guid roleId);
        Task RemoveRoleAsync(Guid userId, Guid roleId);
    }
}
