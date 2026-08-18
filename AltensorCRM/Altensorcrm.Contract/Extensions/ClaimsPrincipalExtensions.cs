using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Altensorcrm.Contract.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var val = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? user.FindFirst("sub")?.Value
               ?? user.FindFirst("id")?.Value;

        return Guid.TryParse(val, out var id) ? id : null;
    }

    public static Guid? GetTenantId(this ClaimsPrincipal user)
    {
        var val = user.FindFirst("tenant_id")?.Value
               ?? user.FindFirst("tenantid")?.Value;

        return Guid.TryParse(val, out var id) ? id : null;
    }

    public static string GetTenantStatus(this ClaimsPrincipal user)
        => user.FindFirst("tenant_status")?.Value ?? "Unknown";

    public static bool HasPermission(this ClaimsPrincipal user, string permissionCode)
        => user.FindAll("permissions").Any(c => string.Equals(c.Value, permissionCode, StringComparison.OrdinalIgnoreCase));

    public static bool HasModule(this ClaimsPrincipal user, string moduleCode)
        => user.FindAll("modules").Any(c => string.Equals(c.Value, moduleCode, StringComparison.OrdinalIgnoreCase));

    public static List<string> GetRoles(this ClaimsPrincipal user)
        => user.FindAll(ClaimTypes.Role).Concat(user.FindAll("roles")).Concat(user.FindAll("role")).Select(c => c.Value).Distinct().ToList();
}
