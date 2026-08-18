using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Altensorcrm.Contract.Services.Tenant;
using Altensorcrm.Contract.Services.User;
using Microsoft.AspNetCore.Http;

namespace Altensorcrm.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService, ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var val = User?.FindFirstValue(ClaimTypes.NameIdentifier) 
                   ?? User?.FindFirstValue("sub")
                   ?? User?.FindFirstValue("id");
            return Guid.TryParse(val, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var val = User?.FindFirstValue("tenant_id")
                   ?? User?.FindFirstValue("tenantid");
            return Guid.TryParse(val, out var id) ? id : null;
        }
    }

    public string? TenantStatus => User?.FindFirstValue("tenant_status");
    public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email");

    public IEnumerable<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value)
                                     ?? User?.FindAll("roles").Select(c => c.Value)
                                     ?? Enumerable.Empty<string>();

    public IEnumerable<string> Permissions => User?.FindAll("permission").Select(c => c.Value)
                                           ?? User?.FindAll("permissions").Select(c => c.Value)
                                           ?? Enumerable.Empty<string>();

    public IEnumerable<string> Modules => User?.FindAll("module").Select(c => c.Value)
                                       ?? User?.FindAll("modules").Select(c => c.Value)
                                       ?? Enumerable.Empty<string>();

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

    public bool HasModuleAccess(string moduleCode) =>
        Modules.Contains(moduleCode, StringComparer.OrdinalIgnoreCase);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public bool IsPlatformSuperAdmin => User?.IsInRole("PlatformSuperAdmin") == true;
    public bool IsTenantAdmin => User?.IsInRole("TenantAdmin") == true;
}

