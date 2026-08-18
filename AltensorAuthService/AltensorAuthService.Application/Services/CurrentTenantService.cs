using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AltensorAuthService.Application.Services
{
    public class CurrentTenantService : ICurrentTenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? TenantId
        {
            get
            {
                var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimTypes.TenantId)?.Value
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;

                if (Guid.TryParse(tenantIdClaim, out var tenantId))
                {
                    return tenantId;
                }

                return null;
            }
        }

        public string? TenantStatus =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimTypes.TenantStatus)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_status")?.Value;

        public Guid? UserId
        {
            get
            {
                var subClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

                if (Guid.TryParse(subClaim, out var userId))
                {
                    return userId;
                }

                return null;
            }
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

        public bool IsPlatformSuperAdmin =>
            _httpContextAccessor.HttpContext?.User?.IsInRole("PlatformSuperAdmin") == true;

        public bool IsTenantAdmin =>
            _httpContextAccessor.HttpContext?.User?.IsInRole("TenantAdmin") == true;
    }
}
