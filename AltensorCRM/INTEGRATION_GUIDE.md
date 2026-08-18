# AltensorAuthService — Modul İnteqrasiya Bələdçisi

> **Versiya:** 1.0  
> **Tarix:** 2026-08-18  
> **Hazırlandı:** AltensorAuthService arxitekturası əsasında  

---

## Mündəricat

1. [Ümumi Arxitektura](#1-ümumi-arxitektura)
2. [JWT Doğrulama (Token Validation)](#2-jwt-doğrulama-token-validation)
3. [JWKS Public Key Endpoint](#3-jwks-public-key-endpoint)
4. [İstifadəçi Kontekstini Oxumaq (Claims)](#4-istifadəçi-kontekstini-oxumaq-claims)
5. [Tenant İzolyasiyası](#5-tenant-izolyasiyası)
6. [İcazə (Permission) Yoxlaması](#6-icazə-permission-yoxlaması)
7. [Modul Abunəliyi Yoxlaması](#7-modul-abunəliyi-yoxlaması)
8. [Integration Events — UserCreated](#8-integration-events--usercreated)
9. [Auth Service API Endpointləri](#9-auth-service-api-endpointləri)
10. [Modul Endpoint Qeydiyyatı (appsettings)](#10-modul-endpoint-qeydiyyatı-appsettings)
11. [Nümunə Modul Quraşdırması (C# ASP.NET Core)](#11-nümunə-modul-quraşdırması-c-aspnet-core)
12. [Yol Xəritəsi (Gələcək Planlar)](#12-yol-xəritəsi-gələcək-planlar)

---

## 1. Ümumi Arxitektura

```
+----------------------------------------------------------+
|                   CLIENT (Browser/App)                    |
+---------------------------+------------------------------+
                            |  POST /api/auth/login
                            v
+----------------------------------------------------------+
|              AltensorAuthService  (Port: 5XXX)            |
|                                                           |
|  * JWT Token uretiir (RS256, RSA Private Key ile)         |
|  * Tenant, User, Role, Permission, Module idarə edir      |
|  * JWKS endpoint: /.well-known/jwks.json                  |
+-------+----------------------------------+----------------+
        |  Bearer Token (JWT)              |  Integration Event
        |  verilir cliente                 |  (HTTP callback)
        v                                  v
+------------------+            +----------------------+
|   CRM Module     |            |   HR Module          |
|  (oz veritabani) |            |  (oz veritabani)     |
|                  |            |                      |
| JWT -> dogrula   |            | JWT -> dogrula       |
| Claims -> oxu    |            | Claims -> oxu        |
| Tenant izol et   |            | Tenant izol et       |
+------------------+            +----------------------+
```

**Əsas Prinsip:** Auth Service token yaradır, digər modullar yalnız token doğrulayır. Hər modul öz veritabanını saxlayır, lakin istifadəçi kimliyi həmişə Auth Service-dən gəlir.

---

## 2. JWT Doğrulama (Token Validation)

### Token formatı

Auth Service **RS256** (RSA imza) istifadə edir. Token aşağıdakı claim-ləri ehtiva edir:

| Claim | Dəyər | Açıqlama |
|-------|-------|----------|
| `sub` | `Guid` | İstifadəçi ID-si (`ApplicationUser.Id`) |
| `email` | `string` | İstifadəçi email-i |
| `tenant_id` | `Guid` | Tenant ID-si |
| `tenant_slug` | `string` | Tenant slug (məs. `"abc-company"`) |
| `tenant_name` | `string` | Tenant adı |
| `roles` | `string[]` | Rol adları |
| `permissions` | `string[]` | İcazə kodları |
| `modules` | `string[]` | Abunə olan modul kodları |
| `iss` | `"AltensorAuthService"` | Issuer |
| `aud` | `"AltensorPlatform"` | Audience |
| `exp` | Unix timestamp | Token bitmə tarixi (15 dəq) |

### NuGet Paketləri

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.*" />
```

### Program.cs konfiqurasiyası

```csharp
// Program.cs — digər modul (məs. TMS, CRM, HR)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Dev mühiti üçün
        options.MapInboundClaims = false;     // Claim adlarını olduğu kimi saxlayır (sub, permissions, modules və s.)
        options.RefreshOnIssuerKeyNotFound = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = "AltensorAuthService",
            ValidateAudience = true,
            ValidAudience = "AltensorPlatform",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Auth service-in JWKS endpoint-i (avtomatik keşləmə ilə)
        var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            builder.Configuration["AuthService:JwksUrl"]!,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever(httpClient) { RequireHttps = false }
        );
    });
```

---

## 3. JWKS Public Key Endpoint

Auth Service RSA açıq açarını standart JWKS formatında paylaşır:

```
GET /.well-known/jwks.json
Authorization: Tələb olunmur (anonymous)
```

**Cavab:**
```json
{
  "keys": [
    {
      "kty": "RSA",
      "use": "sig",
      "alg": "RS256",
      "kid": "altensor-auth-key-1",
      "n": "<base64url modulus>",
      "e": "<base64url exponent>"
    }
  ]
}
```

Modul bu endpoint-i startup-da yükləməli və caching tətbiq etməlidir:

```csharp
// AddJwtBearer içinde:
options.RefreshOnIssuerKeyNotFound = true; // Key deyisense avtomatik yenile
```

---

## 4. İstifadəçi Kontekstini Oxumaq (Claims)

Token doğrulandıqdan sonra claim-ləri belə oxuyun:

```csharp
// YourController.cs
[Authorize]
[HttpGet("my-data")]
public IActionResult GetMyData()
{
    var userId     = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var email      = User.FindFirstValue(ClaimTypes.Email)!;
    var tenantId   = Guid.Parse(User.FindFirstValue("tenant_id")!);
    var tenantSlug = User.FindFirstValue("tenant_slug")!;

    var roles       = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    var permissions = User.FindAll("permissions").Select(c => c.Value).ToList();
    var modules     = User.FindAll("modules").Select(c => c.Value).ToList();

    // Yalnız bu tenant-a aid məlumatlar
    var myData = _repo.GetByTenantId(tenantId);
    return Ok(myData);
}
```

### Yardımçı Extension metodu

```csharp
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static Guid GetTenantId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue("tenant_id")!);

    public static bool HasPermission(this ClaimsPrincipal user, string permissionCode)
        => user.FindAll("permissions").Any(c => c.Value == permissionCode);

    public static bool HasModule(this ClaimsPrincipal user, string moduleCode)
        => user.FindAll("modules").Any(c => c.Value == moduleCode);
}
```

---

## 5. Tenant İzolyasiyası

**Qızıl qayda:** Hər sorğuda `tenant_id` claim-ini yoxlayın. Veritabanı sorğularında həmişə tenant filtri tətbiq edin.

```csharp
// Middleware kimi elave etmek tovsiye olunur
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ICurrentTenantContext tenantCtx)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirstValue("tenant_id");
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                tenantCtx.SetTenant(tenantId);
            }
        }
        await _next(context);
    }
}
```

```csharp
// Repository-de
public async Task<List<Order>> GetOrdersAsync(Guid tenantId)
    => await _db.Orders
        .Where(o => o.TenantId == tenantId)  // <- Hemise!
        .ToListAsync();
```

---

## 6. İcazə (Permission) Yoxlaması

Auth Service JWT-yə `permissions` claim-lərini yerləşdirir. Modullar bu claim-ləri oxuyaraq icazəni yoxlaya bilər.

### Hazır Policy əlavəsi

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewOrders",
        p => p.RequireClaim("permissions", "orders.view"));

    options.AddPolicy("CanCreateOrder",
        p => p.RequireClaim("permissions", "orders.create"));

    options.AddPolicy("CanDeleteOrder",
        p => p.RequireClaim("permissions", "orders.delete"));
});
```

```csharp
// Controller-de istifade
[Authorize(Policy = "CanViewOrders")]
[HttpGet("orders")]
public IActionResult GetOrders() { ... }
```

---

## 7. Modul Abunəliyi Yoxlaması

Token içindəki `modules` claim-i həmin tenant-ın hansı modullara abunə olduğunu göstərir.

```csharp
// Middleware ile modul girisini qoru
public class ModuleAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _requiredModule;

    public ModuleAccessMiddleware(RequestDelegate next, string requiredModule)
    {
        _next = next;
        _requiredModule = requiredModule;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var modules = context.User.FindAll("modules").Select(c => c.Value);
            if (!modules.Contains(_requiredModule))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = $"Bu tenant '{_requiredModule}' moduluna abune deyil."
                });
                return;
            }
        }
        await _next(context);
    }
}

// Program.cs-de qeydiyyat (CRM modulu ucun)
app.UseMiddleware<ModuleAccessMiddleware>("CRM");
```

### Alternativ — Policy ile

```csharp
options.AddPolicy("CrmModuleAccess",
    p => p.RequireClaim("modules", "CRM"));

[Authorize(Policy = "CrmModuleAccess")]
[ApiController]
[Route("api/crm")]
public class CrmController : ControllerBase { ... }
```

---

## 8. Integration Events — UserCreated

Auth Service-də yeni istifadəçi yaradılanda `IIntegrationEventPublisher` vasitəsilə digər modullara HTTP sorğusu göndərilir.

### Event strukturu

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tenantId": "7a9a4d0b-cd68-4e17-a1c0-4d93a57c4971",
  "email": "ali@company.com",
  "fullName": "Əli Həsənov",
  "createdAt": "2026-08-18T09:00:00Z"
}
```

### Modulun qəbul etdiyi Webhook Endpoint

Hər modul aşağıdakı endpoint-i implement etməlidir:

```csharp
// YourModule/Controllers/AuthWebhookController.cs
[ApiController]
[Route("internal/webhooks")]
public class AuthWebhookController : ControllerBase
{
    private readonly IUserSyncService _userSync;

    public AuthWebhookController(IUserSyncService userSync)
        => _userSync = userSync;

    /// <summary>
    /// Auth Service-den yeni user yaradildi bilirisi
    /// POST /internal/webhooks/user-created
    /// </summary>
    [HttpPost("user-created")]
    public async Task<IActionResult> OnUserCreated([FromBody] UserCreatedPayload payload)
    {
        // Modulun oz veritabaninda user profil yarat
        await _userSync.SyncNewUserAsync(
            payload.UserId,
            payload.TenantId,
            payload.Email,
            payload.FullName);
        return Ok();
    }
}

public class UserCreatedPayload
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = default!;
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Auth Service appsettings.json-a modul URL elave et

```json
{
  "ModuleEndpoints": {
    "crm": "https://crm-service.altensor.com",
    "hr": "https://hr-service.altensor.com",
    "inventory": "https://inventory-service.altensor.com"
  }
}
```

> **Qeyd:** `Module.Code` (kicik herflə) ile `ModuleEndpoints` acarlari uygun olmalidir.
> Auth service `POST {moduleUrl}/internal/webhooks/user-created` cagiracaq.

---

## 9. Auth Service API Endpointləri

### Autentifikasiya

| Metod | URL | İcazə | Açıqlama |
|-------|-----|-------|----------|
| `POST` | `/api/auth/login` | Anonim | Email + şifrə + slug ilə login |
| `POST` | `/api/auth/refresh` | Anonim | Refresh token ilə yeni JWT |
| `POST` | `/api/auth/logout` | JWT | Cari cihazdan çıxış |
| `POST` | `/api/auth/logout-all` | JWT | Bütün cihazlardan çıxış |
| `GET`  | `/api/auth/me` | JWT | Cari user məlumatları |
| `POST` | `/api/auth/forgot-password` | Anonim | OTP göndər |
| `POST` | `/api/auth/reset-password` | Anonim | OTP ilə şifrə sıfırla |

### JWKS

| Metod | URL | İcazə | Açıqlama |
|-------|-----|-------|----------|
| `GET` | `/.well-known/jwks.json` | Anonim | RSA Public Key (JWKS formatı) |

### Tenant İdarəetməsi

| Metod | URL | Rol | Açıqlama |
|-------|-----|-----|----------|
| `POST` | `/api/tenant` | PlatformSuperAdmin | Yeni tenant + admin yarat |
| `GET`  | `/api/tenant/{id}` | TenantAdmin+ | Tenant detalları |
| `POST` | `/api/tenant/{id}/suspend` | PlatformSuperAdmin | Tenantı dondur |
| `POST` | `/api/tenant/{id}/activate` | PlatformSuperAdmin | Tenantı aktiv et |
| `POST` | `/api/tenant/{id}/modules` | TenantAdmin | Modul abunəliyi əlavə et |

---

## 10. Modul Endpoint Qeydiyyatı (appsettings)

Auth Service-in `appsettings.json`-da `ModuleEndpoints` bolmesine yeni modulu elave edin:

```json
{
  "ModuleEndpoints": {
    "crm": "https://crm.altensor.com",
    "hr": "https://hr.altensor.com",
    "inventory": "https://inventory.altensor.com",
    "yeni_modul": "https://yeni-modul.altensor.com"
  }
}
```

**Lokal development ucun:**

```json
{
  "ModuleEndpoints": {
    "tms": "https://localhost:7288/internal/webhooks/user-created",
    "crm": "http://localhost:5200",
    "hr": "http://localhost:5300"
  }
}
```

---

## 11. Nümunə Modul Quraşdırması (C# ASP.NET Core)

Tam bir modul layihəsi üçün minimal `Program.cs`:

```csharp
// YourModule/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Auth: Auth Service-in JWKS-den public key yukle
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidIssuer   = "AltensorAuthService",
            ValidAudience = "AltensorPlatform",
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // JWKS endpointden public key al
        options.ConfigurationManager =
            new ConfigurationManager<OpenIdConnectConfiguration>(
                builder.Configuration["AuthService:JwksUrl"]!,
                new OpenIdConnectConfigurationRetriever(),
                TimeSpan.FromHours(24));
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Modul erisimi
    options.AddPolicy("ModuleAccess", p => p.RequireClaim("modules", "CRM"));
    // CRUD icazeler
    options.AddPolicy("CanView",   p => p.RequireClaim("permissions", "crm.view"));
    options.AddPolicy("CanCreate", p => p.RequireClaim("permissions", "crm.create"));
    options.AddPolicy("CanUpdate", p => p.RequireClaim("permissions", "crm.update"));
    options.AddPolicy("CanDelete", p => p.RequireClaim("permissions", "crm.delete"));
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantContextMiddleware>();

app.MapControllers();
app.Run();
```

### appsettings.json (modul terəfi)

```json
{
  "AuthService": {
    "BaseUrl": "http://localhost:5000",
    "JwksUrl": "http://localhost:5000/.well-known/jwks.json"
  }
}
```

---

## 12. Yol Xəritəsi (Gələcək Planlar)

Hal-hazırda `IIntegrationEventPublisher` **HTTP** ilə işləyir. Gələcəkdə mesaj broku istifadəsi planlaşdırılır:

```
Hazirda:
  AuthService --> HTTP POST --> Module Webhook

Planlasdirilmis:
  AuthService --> RabbitMQ / Azure Service Bus --> Module Consumer
```

### Gələcək Əlavə Etmək Planlanmış Eventlər

| Event | Açıqlama | Status |
|-------|----------|--------|
| `UserCreatedIntegrationEvent` | Yeni user yaradıldı | Hazır |
| `UserDeactivatedIntegrationEvent` | User deaktiv edildi | Gözləyir |
| `TenantSuspendedIntegrationEvent` | Tenant donduruldu | Gözləyir |
| `ModuleSubscriptionAddedEvent` | Modul abunəliyi əlavə edildi | Gözləyir |
| `ModuleSubscriptionExpiredEvent` | Modul abunəliyi bitdi | Gözləyir |
| `RolePermissionsChangedEvent` | Rol icazələri dəyişdi | Gözləyir |

### Gələcəkdə MassTransit ilə Consumer Nümunəsi

```csharp
// Modul terəfindeki gelecek implementasiya
public class UserCreatedConsumer : IConsumer<UserCreatedIntegrationEvent>
{
    private readonly IUserProfileService _userProfileService;

    public UserCreatedConsumer(IUserProfileService userProfileService)
        => _userProfileService = userProfileService;

    public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
    {
        var evt = context.Message;
        // Modulun oz veritabaninda user profil yarat
        await _userProfileService.CreateProfileAsync(
            evt.UserId, evt.TenantId, evt.Email, evt.FullName);
    }
}
```

---

## Sürətli Yoxlama Siyahısı (Checklist)

Yeni modul inteqrasiya edilərkən bu addımları yoxlayın:

- [ ] `/.well-known/jwks.json` URL-i konfiqurasiyaya əlavə edilib
- [ ] `AddAuthentication` + `AddJwtBearer` `Program.cs`-ə əlavə edilib
- [ ] `ValidIssuer = "AltensorAuthService"` düzgün yazılıb
- [ ] `ValidAudience = "AltensorPlatform"` düzgün yazılıb
- [ ] Bütün veritabanı sorğularına `TenantId` filtri əlavə edilib
- [ ] `POST /internal/webhooks/user-created` endpoint implement edilib
- [ ] Auth Service `appsettings.json`-da `ModuleEndpoints` bölməsinə modul URL əlavə edilib
- [ ] `Module.Code` (kiçik hərflə) ilə `ModuleEndpoints` açarı uyğundur
- [ ] Authorization policy-lər modulun öz icazə kodlarına uyğundur
- [ ] Lokal testdə Auth Service çalışır (`http://localhost:5000`)

---

*Bu sənəd `AltensorAuthService` arxitekturası əsasında hazırlanmışdır.*
