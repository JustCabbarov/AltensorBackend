using Contract.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class AuthorizationService : IAuthorizeService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAppUserRepository _appUserRepository;
        private readonly ICurrentTenantService _tenantService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            IUserRepository userRepository,
            IAppUserRepository appUserRepository,
            ICurrentTenantService tenantService,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<AuthorizationService> logger)
        {
            _userRepository = userRepository;
            _appUserRepository = appUserRepository;
            _tenantService = tenantService;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            var tenantId = _tenantService.TenantId
                ?? throw new UnauthorizedAccessException("Tenant konteksti tapılmadı.");

            // Auth Service-dən müştərinin bütün istifadəçilərini canlı çəkib TMS bazasına sinxronlaşdır
            try
            {
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    var authBaseUrl = _configuration["AuthService:BaseUrl"] ?? "https://api-info.altensor.com";
                    var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Add("Authorization", authHeader);

                    var response = await client.GetAsync($"{authBaseUrl.TrimEnd('/')}/api/Tenant/users");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in doc.RootElement.EnumerateArray())
                            {
                                var idStr = item.GetProperty("id").GetString();
                                var email = item.TryGetProperty("email", out var eProp) ? eProp.GetString() : null;
                                var fullName = item.TryGetProperty("fullName", out var fnProp) ? fnProp.GetString() : null;

                                if (Guid.TryParse(idStr, out var userId) && !string.IsNullOrEmpty(email))
                                {
                                    await _appUserRepository.EnsureExistsAsync(
                                        userId,
                                        tenantId,
                                        email,
                                        fullName ?? email,
                                        email
                                    );
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Auth Service-dən istifadəçilər sinxronlaşdırılarkən xəbərdarlıq.");
            }

            _logger.LogInformation("GetAllUsers: TenantId={TenantId}", tenantId);
            return await _userRepository.GetAllUsersAsync(tenantId);
        }

        public async Task<AppUser?> GetUserByIdAsync(Guid userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }
    }
}
