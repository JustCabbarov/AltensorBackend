using System;
using System.Collections.Generic;
using System.Threading;
using Altensorcrm.Contract.DTOs.UserManagement;

namespace Altensorcrm.Contract.Services.UserManagement;

public interface IUserService
{
    System.Threading.Tasks.Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<UserDto?> UpdateProfileAsync(Guid id, UpdateUserProfileDto dto, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<string?> UpdateAvatarAsync(Guid id, string avatarUrl, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<object> GetSalesHierarchyAsync(CancellationToken cancellationToken = default);
}
