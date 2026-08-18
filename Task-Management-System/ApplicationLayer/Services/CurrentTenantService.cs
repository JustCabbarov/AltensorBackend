using Contract.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Application.Services
{
    /// <summary>
    /// ICurrentTenantService implementasiyası.
    /// JWT token-indəki claim-ləri IHttpContextAccessor vasitəsilə oxuyur.
    /// </summary>
    public class CurrentTenantService : ICurrentTenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        private ClaimsPrincipal? User
            => _httpContextAccessor.HttpContext?.User;

        /// <summary>JWT-dəki "tenant_id" claim-ini oxu</summary>
        public Guid? TenantId
        {
            get
            {
                var val = User?.FindFirstValue("tenant_id");
                return Guid.TryParse(val, out var id) ? id : null;
            }
        }

        /// <summary>JWT-dəki "sub" və ya "nameid" claim-ini oxu</summary>
        public Guid? UserId
        {
            get
            {
                var val = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User?.FindFirstValue("sub");
                return Guid.TryParse(val, out var id) ? id : null;
            }
        }

        public string? TenantStatus
            => User?.FindFirstValue("tenant_status");

        public bool IsAuthenticated
            => User?.Identity?.IsAuthenticated == true;

        public bool IsPlatformSuperAdmin
            => User?.IsInRole("PlatformSuperAdmin") == true;

        public bool IsTenantAdmin
            => User?.IsInRole("TenantAdmin") == true;
    }
}
