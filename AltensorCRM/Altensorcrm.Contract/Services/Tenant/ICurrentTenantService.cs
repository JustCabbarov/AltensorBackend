using System;

namespace Altensorcrm.Contract.Services.Tenant;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
    string? TenantStatus { get; }
    bool IsAuthenticated { get; }
    bool IsPlatformSuperAdmin { get; }
    bool IsTenantAdmin { get; }
}
