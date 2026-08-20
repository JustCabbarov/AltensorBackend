using System;

namespace Contract.Services
{
    /// <summary>
    /// JWT token-indən cari sorğunun tenant kontekstini oxuyan servis.
    /// Bütün layerlər bu interface vasitəsilə tenant məlumatına çatır.
    /// </summary>
    public interface ICurrentTenantService
    {
        /// <summary>JWT-dəki "tenant_id" claim-i</summary>
        Guid? TenantId { get; }

        /// <summary>JWT-dəki "sub" / "nameid" claim-i</summary>
        Guid? UserId { get; }

        /// <summary>JWT-dəki "tenant_status" claim-i: Active | Trial | Suspended | Expired</summary>
        string? TenantStatus { get; }

        string? Email { get; }
        System.Collections.Generic.IEnumerable<string> Roles { get; }
        System.Collections.Generic.IEnumerable<string> Permissions { get; }
        System.Collections.Generic.IEnumerable<string> Modules { get; }

        bool HasPermission(string permission);
        bool HasModuleAccess(string moduleCode);

        bool IsAuthenticated { get; }
        bool IsPlatformSuperAdmin { get; }
        bool IsTenantAdmin { get; }
    }
}
