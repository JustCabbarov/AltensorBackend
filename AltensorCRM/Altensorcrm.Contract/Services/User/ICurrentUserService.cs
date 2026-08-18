using System;
using System.Collections.Generic;

namespace Altensorcrm.Contract.Services.User;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? TenantStatus { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
    IEnumerable<string> Modules { get; }
    bool HasPermission(string permission);
    bool HasModuleAccess(string moduleCode);
}
