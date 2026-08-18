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

        bool IsAuthenticated { get; }
        bool IsPlatformSuperAdmin { get; }
        bool IsTenantAdmin { get; }
    }
}
