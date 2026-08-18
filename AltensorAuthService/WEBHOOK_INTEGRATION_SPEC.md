# Altensor Platform — Modul İnteqrasiyası üzrə Tam Bələdçi (JWT Token & Webhook)

Bu sənəd **Altensor Platform** ekosistemində yerləşən hər hansı bir alt modulun (məs: *TMS, CRM, HRM, Inventory, Finance və s.*) **AltensorAuthService** ilə inteqrasiya olunması üçün texniki bələdçidir. 

Sənəd iki əsas hissədən ibarətdir:
1. **Bölmə A: JWT Token Doğrulaması (Authentication & Multi-Tenant Authorization)**
2. **Bölmə B: Webhook ilə İstifadəçi Sinxronizasiyası (User Synchronization Webhook)**

---

# BÖLMƏ A: JWT Token İnteqrasiyası (Auth & Claims)

Altensor ekosistemində mikroservislər arasında mərkəzsiz və yüksək performanslı token yoxlanışı üçün **Asimmetrik RSA (RS256)** alqoritmi istifadə olunur. Modul hər sorğuda Auth Service-ə müraciət etmədən, Auth Service-in `.well-known/jwks.json` endpoint-indən publik açarı alaraq token-i lokal şəkildə doğrulamalıdır.

---

## 1. Token Strukturu və Claim-lər

`AltensorAuthService` tərəfindən generasiya olunan Access Token daxilindəki əsas claim-lər:

| Claim Adı | Tip | Təsvir / İstifadə Məqsədi | Nümunə |
|---|---|---|---|
| `sub` / `nameid` | `Guid` (string) | İstifadəçinin qlobal identifikatoru (`UserId`) | `e2a3b4c5-6789-4abc-def0-1234567890ab` |
| `tenant_id` | `Guid` (string) | İstifadəçinin aid olduğu şirkət/təşkilat (`TenantId`) | `a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d` |
| `tenant_status` | `string` | Tenant-ın cari vəziyyəti (`Active`, `Suspended`, `Passive`) | `Active` |
| `email` | `string` | İstifadəçinin email ünvanı | `user@example.com` |
| `name` | `string` | İstifadəçinin tam adı | `Əli Məmmədov` |
| `roles` / `role` | `string[]` | İstifadəçiyə verilmiş rollar | `["TenantAdmin", "Manager"]` |
| `permissions` / `permission` | `string[]` | İstifadəçinin xüsusi hüquqları | `["tms.tasks.create", "tms.tasks.delete"]` |
| `modules` / `module` | `string[]` | İstifadəçinin/Tenant-ın daxil ola biləcəyi aktiv modullar | `["tms", "crm"]` |

---

## 2. Modulda JWT Bearer Doğrulamasının Qurulması (.NET Nümunəsi)

### Addım 1: NuGet Paketlərini Əlavə Edin
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.*" />
<PackageReference Include="Microsoft.IdentityModel.Protocols.OpenIdConnect" Version="7.5.*" />
```

### Addım 2: Modulun `appsettings.json` Konfiqurasiyası
```json
{
  "Jwt": {
    "Issuer": "AltensorAuthService",
    "Audience": "AltensorPlatform",
    "Authority": "https://localhost:7196" 
  },
  "AuthService": {
    "BaseUrl": "https://localhost:7196",
    "JwksEndpoint": "https://localhost:7196/.well-known/jwks.json"
  }
}
```

### Addım 3: JWKS Əsaslı Avtomatik Token Doğrulama Setup-ı
`Program.cs` və ya Service Collection Extension faylında JWT konfiqurasiyasını qeydiyyatdan keçirin:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Security.Cryptography;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAltensorAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "AltensorAuthService";
        var audience = configuration["Jwt:Audience"] ?? "AltensorPlatform";
        var jwksUrl = configuration["AuthService:JwksEndpoint"] ?? "https://localhost:7196/.well-known/jwks.json";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Dev mühitində false, Prod-da true
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                ValidateIssuerSigningKey = true,

                // JWKS vasitəsilə Auth Service-dən açarı dinamik əldə edir:
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                {
                    try
                    {
                        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                        var response = httpClient.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
                        
                        using var doc = JsonDocument.Parse(response);
                        var keys = doc.RootElement.GetProperty("keys");

                        foreach (var key in keys.EnumerateArray())
                        {
                            var currentKid = key.GetProperty("kid").GetString();
                            if (currentKid == kid || string.IsNullOrEmpty(kid))
                            {
                                var n = key.GetProperty("n").GetString()!;
                                var e = key.GetProperty("e").GetString()!;

                                var rsaParams = new RSAParameters
                                {
                                    Modulus = Base64UrlEncoder.DecodeBytes(n),
                                    Exponent = Base64UrlEncoder.DecodeBytes(e)
                                };

                                var rsa = RSA.Create();
                                rsa.ImportParameters(rsaParams);
                                return new[] { new RsaSecurityKey(rsa) { KeyId = currentKid } };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Auth] JWKS açarları oxunarkən xəta: {ex.Message}");
                    }

                    return Enumerable.Empty<SecurityKey>();
                }
            };
        });

        services.AddAuthorization();
        return services;
    }
}
```

---

## 3. Modul Daxilində `CurrentTenant` və `CurrentUser` Məlumatlarını Oxumaq

Hər bir modul daxilində cari istifadəçinin və təşkilatın (Tenant) kontekstini əldə etmək üçün `ICurrentUserService` servisi yaradın:

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace YourModule.Application.Interfaces
{
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
}
```

**İmplementasiyası:**
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using YourModule.Application.Interfaces;

namespace YourModule.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? UserId
        {
            get
            {
                var val = User?.FindFirstValue(ClaimTypes.NameIdentifier) 
                       ?? User?.FindFirstValue("sub");
                return Guid.TryParse(val, out var id) ? id : null;
            }
        }

        public Guid? TenantId
        {
            get
            {
                var val = User?.FindFirstValue("tenant_id");
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
    }
}
```

---

# BÖLMƏ B: Webhook İnteqrasiyası (User Sync)

`AltensorAuthService`-də yeni istifadəçi qeydiyyatdan keçdikdə və ya yaradıldıqda, sistem avtomatik olaraq həmin istifadəçinin şirkətinin (Tenant) aktiv abunə olduğu bütün alt modullara paralel olaraq `POST` webhook bildirişi göndərir.

---

## 1. Webhook Sorğu Spesifikasiyası

- **Method:** `POST`
- **Default Path:** `/internal/webhooks/user-created`
- **Content-Type:** `application/json; charset=utf-8`
- **Təhlükəsizlik Header-ləri:** 
  - `X-Webhook-Secret`
  - `X-Internal-Api-Key`
- **Secret Dəyəri:** `Altensor_Internal_Secret_Key_2026_Secure!`

### Webhook JSON Payload:
```json
{
  "userId": "e2a3b4c5-6789-4abc-def0-1234567890ab",
  "tenantId": "a1b2c3d4-e5f6-4a1b-8c2d-3e4f5a6b7c8d",
  "email": "user@example.com",
  "fullName": "Əli Məmmədov",
  "userName": "alimammadov",
  "createdAt": "2026-08-18T12:00:00Z"
}
```

---

## 2. Modulda Webhook Qəbulunun Qurulması

### Addım 1: DTO Model
```csharp
namespace YourModule.Application.DTOs
{
    public class UserCreatedIntegrationEvent
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

### Addım 2: Webhook Controller
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YourModule.Application.DTOs;
using YourModule.Application.Interfaces;

namespace YourModule.Presentation.Controllers
{
    [ApiController]
    [Route("internal/webhooks")]
    public class InternalWebhooksController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserSyncService _userSyncService;
        private readonly ILogger<InternalWebhooksController> _logger;

        public InternalWebhooksController(
            IConfiguration configuration,
            IUserSyncService userSyncService,
            ILogger<InternalWebhooksController> logger)
        {
            _configuration = configuration;
            _userSyncService = userSyncService;
            _logger = logger;
        }

        [HttpPost("user-created")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> HandleUserCreated(
            [FromBody] UserCreatedIntegrationEvent @event,
            [FromHeader(Name = "X-Webhook-Secret")] string? webhookSecret,
            [FromHeader(Name = "X-Internal-Api-Key")] string? apiKey,
            CancellationToken cancellationToken)
        {
            // 1. Təhlükəsizlik Açarının Yoxlanılması
            var expectedSecret = _configuration["Webhook:SharedSecret"] 
                              ?? _configuration["InternalCommunication:ApiKey"];

            var incomingSecret = !string.IsNullOrWhiteSpace(webhookSecret) ? webhookSecret : apiKey;

            if (string.IsNullOrWhiteSpace(expectedSecret) || incomingSecret != expectedSecret)
            {
                _logger.LogWarning("İcazəsiz webhook sorğusu cəhdi. UserId: {UserId}", @event?.UserId);
                return Unauthorized(new { message = "Unauthorized webhook request." });
            }

            if (@event == null || @event.UserId == Guid.Empty || @event.TenantId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid event payload." });
            }

            // 2. İstifadəçi Sinxronizasiyasının İcrası
            _logger.LogInformation("Webhook qəbul olundu: UserId={UserId}, TenantId={TenantId}, Email={Email}", 
                @event.UserId, @event.TenantId, @event.Email);

            await _userSyncService.SyncUserCreatedAsync(@event, cancellationToken);

            return Ok(new { success = true });
        }
    }
}
```

### Addım 3: Idempotent Sinxronizasiya Servisi
Modulun öz verilənlər bazasında `Users` (və ya `AppUsers`) cədvəlini yeniləyən servis:

```csharp
using Microsoft.EntityFrameworkCore;
using YourModule.Application.DTOs;
using YourModule.Application.Interfaces;
using YourModule.Domain.Entities;
using YourModule.Persistence.Context;

namespace YourModule.Infrastructure.Services
{
    public class UserSyncService : IUserSyncService
    {
        private readonly ModuleDbContext _dbContext;
        private readonly ILogger<UserSyncService> _logger;

        public UserSyncService(ModuleDbContext dbContext, ILogger<UserSyncService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SyncUserCreatedAsync(UserCreatedIntegrationEvent @event, CancellationToken ct)
        {
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == @event.UserId, ct);

            if (existingUser != null)
            {
                // İstifadəçi artıq varsa yeniləyirik (Idempotent)
                existingUser.Email = @event.Email;
                existingUser.FullName = @event.FullName;
                existingUser.UserName = @event.UserName;
                existingUser.TenantId = @event.TenantId;
                _logger.LogInformation("Mövcud istifadəçi məlumatları yeniləndi: {UserId}", @event.UserId);
            }
            else
            {
                // Yeni istifadəçi yaradırıq
                var newUser = new User
                {
                    Id = @event.UserId,
                    TenantId = @event.TenantId,
                    Email = @event.Email,
                    FullName = @event.FullName,
                    UserName = @event.UserName,
                    CreatedAt = @event.CreatedAt
                };
                await _dbContext.Users.AddAsync(newUser, ct);
                _logger.LogInformation("Yeni istifadəçi modul bazasına əlavə edildi: {UserId}", @event.UserId);
            }

            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
```

### Addım 4: Modulun `appsettings.json` Tənzimləmələri
```json
{
  "Webhook": {
    "SharedSecret": "Altensor_Internal_Secret_Key_2026_Secure!"
  },
  "InternalCommunication": {
    "ApiKey": "Altensor_Internal_Secret_Key_2026_Secure!"
  }
}
```

---

# BÖLMƏ C: Auth Service Tərəfində Qeydiyyat (Checklist)

Yeni bir modul platformaya qoşulduqda `AltensorAuthService`-də aşağıdakı 2 addım mütləq icra edilməlidir:

### 1. `appsettings.json`-da Modul Endpoint-i Qeyd Olunmalıdır
`AltensorAuthService.Presentation/appsettings.json` faylında `ModuleEndpoints` bölməsinə həmin modulun ünvanı əlavə edilir:

```json
"ModuleEndpoints": {
  "tms": "https://localhost:7288/internal/webhooks/user-created",
  "crm": "https://localhost:7150/internal/webhooks/user-created",
  "inventory": "https://localhost:7200/internal/webhooks/user-created"
}
```
*(Qeyd: Modul kodu böyük/kiçik hərf fərqi olmadan uyğunlaşdırılır).*

### 2. Verilənlər Bazasında Modul və Abunəlik Qeydi
- **`Modules` cədvəlində:** Modul üçün sətir olmalıdır (məs: `Code = 'crm'`).
- **`TenantModuleSubscriptions` cədvəlində:** İstifadəçinin aid olduğu şirkət (`TenantId`) həmin modula aktiv abunə olmalıdır (`Status = 1` - Active, `IsDeleted = false`).

---

# BÖLMƏ D: Süni İntellekt Agentinə Tapşırıq Təlimatı (AI Agent Prompt)

Əgər bu faylı hər hansı alt modulun (məs: TMS, CRM və s.) repozitoriyasında işləyən AI Agentə verəcəksinizsə, ona aşağıdakı qısa təlimatı göndərə bilərsiniz:

> **"Hörmətli Agent, bu layihə Altensor Platformasının alt moduludur. Zəhmət olmasa təqdim olunmuş spesifikasiyaya uyğun olaraq:
> 1. JWT Bearer Authentication konfiqurasiyasını (RSA JWKS dəstəyi ilə) qur.
> 2. `ICurrentUserService` servisi yaradıb token-dən `UserId`, `TenantId`, `TenantStatus`, `Roles` və `Permissions` claim-lərini oxu.
> 3. `POST /internal/webhooks/user-created` endpoint-ini və `IUserSyncService` servisini idempotent şəkildə implementasiya et.
> 4. `appsettings.json` faylında tələb olunan `Webhook:SharedSecret` və `Jwt` konfiqurasiyalarını tamamla."**
