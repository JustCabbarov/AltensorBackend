using AltensorAuthService.Domain.Enums;
using AltensorAuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AltensorAuthService.Persistence.Integration
{
    /// <summary>
    /// Tenant-ın aktiv abunə olduğu modulların internal endpoint URL-lərini qaytarır.
    /// URL-lər appsettings.json-dakı "ModuleEndpoints" bölməsindən oxunur.
    /// </summary>
    public class ModuleEndpointRegistry
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ModuleEndpointRegistry> _logger;

        public ModuleEndpointRegistry(
            AppDbContext context,
            IConfiguration configuration,
            ILogger<ModuleEndpointRegistry> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Verilmiş tenant üçün "UserCreated" eventini qəbul edən endpoint-lərin tam URL siyahısını qaytarır.
        /// Yalnız aktiv abunəliyi olan və appsettings-də konfiqurasiya edilmiş modullar daxil edilir.
        /// </summary>
        public async Task<List<string>> GetUserCreatedEndpointsAsync(Guid tenantId)
        {
            // Tenant-ın aktiv abunəliklərinin modul kodlarını çək
            var moduleCodes = await _context.TenantModuleSubscriptions
                .Where(s => s.TenantId == tenantId && s.Status == SubscriptionStatus.Active && !s.IsDeleted)
                .Select(s => s.Module.Code)
                .ToListAsync();

            _logger.LogDebug("Tenant {TenantId} üçün {Count} aktiv modul abunəliyi tapıldı.", tenantId, moduleCodes.Count);

            var endpoints = new List<string>();

            foreach (var code in moduleCodes)
            {
                var configuredUrl = _configuration[$"ModuleEndpoints:{code.ToLowerInvariant()}"] 
                                 ?? _configuration[$"ModuleEndpoints:{code}"];

                if (!string.IsNullOrWhiteSpace(configuredUrl))
                {
                    string endpoint;
                    if (configuredUrl.Contains("/internal/") || configuredUrl.Contains("/api/"))
                    {
                        endpoint = configuredUrl;
                    }
                    else
                    {
                        endpoint = $"{configuredUrl.TrimEnd('/')}/internal/webhooks/user-created";
                    }

                    endpoints.Add(endpoint);
                    _logger.LogInformation("Modul webhook endpoint tapıldı: Code={Code}, Endpoint={Endpoint}", code, endpoint);
                }
                else
                {
                    _logger.LogDebug("Modul '{Code}' üçün appsettings-də endpoint tapılmadı — atlanılır.", code);
                }
            }

            return endpoints;
        }
    }
}
