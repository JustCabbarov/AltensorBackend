# Altensor Platform — Multi-Tenant Arxitektura Sənədi
### (Agent üçün hazırlanmış — tam kontekst, hazır kod)

> Bu sənəd **AltensorAuthService**-dəki multi-tenant məntiqi əsasında hazırlanmışdır.
> Yeni bir modul (CRM, HR, Inventory və s.) yazarkən bu sənədi agentə ver —
> agent lazım olan hər şeyi buradan götürüb eyni məntiqi tətbiq edə bilər.

---

## 1. Seçilmiş Yanaşma: Shared Database, Tenant-per-Row

Altensor **shared database** yanaşmasından istifadə edir.  
Bütün tenantların məlumatları eyni veritabanında saxlanır, lakin **hər cədvəldə `TenantId` sütunu** var.

```
Shared PostgreSQL DB
├── Tenants           (hər şirkətin əsas qeydi)
├── Users             (TenantId  ← bağlı)
├── Roles             (TenantId? ← null = sistem rolu, dolu = o tenanta aid)
├── Orders            (TenantId  ← bağlı)  [CRM modulu]
├── Contacts          (TenantId  ← bağlı)  [CRM modulu]
└── ...
```

**Niyə bu yanaşma?**
- Tək veritabanı — idarəetmə asandır
- Hər cərgəyə `TenantId` filter — tam izolyasiya
- Yeni tenant yaradanda heç bir migration tələb olunmur

---

## 2. Tenant Anlayışı (Kim nədir?)

| Termin | Nə deməkdir |
|--------|-------------|
| **Tenant** | Sistemə qeydiyyatdan keçmiş şirkət/müştəri (məs. "ABC MMC") |
| **TenantId** | O şirkətin unikal `Guid` identifikatoru |
| **TenantSlug** | Login üçün istifadə edilən qısa ad (məs. `"abc-mmc"`) |
| **TenantAdmin** | O şirkətin öz adminı — yalnız öz istifadəçilərini idarə edir |
| **PlatformSuperAdmin** | Bütün tenantları idarə edən platform operatoru |

---

## 3. Auth Service-dən Gələn JWT Strukturu

İstifadəçi login olduqda Auth Service RS256 imzalı JWT verir.  
Bu tokenin içindəki **claim-lər** modulun bilməli olduğu hər şeyi ehtiva edir:

```json
{
  "sub":           "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nameid":        "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email":         "ali@abc-mmc.com",
  "name":          "Əli Həsənov",
  "tenant_id":     "7a9a4d0b-cd68-4e17-a1c0-4d93a57c4971",
  "tenant_status": "Active",
  "role":          ["TenantAdmin"],
  "roles":         ["TenantAdmin"],
  "permissions":   ["crm.contacts.view", "crm.contacts.create"],
  "modules":       ["CRM", "HR"],
  "iss":           "AltensorAuthService",
  "aud":           "AltensorPlatform",
  "exp":           1755500000
}
```

### Claim adları (exact strings — bunları istifadə et):

| Claim | Tipi | Məzmun |
|-------|------|--------|
| `sub` / `nameid` | `Guid string` | İstifadəçi ID-si |
| `email` | `string` | Email |
| `name` | `string?` | Ad Soyad |
| `tenant_id` | `Guid string` | Tenant ID — **ən vacib olan** |
| `tenant_status` | `string` | `Active`, `Suspended`, `Trial`, `Expired` |
| `role` / `roles` | `string[]` | Rol adları |
| `permissions` | `string[]` | İcazə kodları (məs. `"crm.contacts.create"`) |
| `modules` | `string[]` | Abunə olan modul kodları (məs. `"CRM"`, `"HR"`) |

---

## 4. Tenant Status Həyat Dövrü

```
           yarananda
              │
              ▼
         [ Trial ]
              │
    aktiv olduqda
              │
              ▼
         [ Active ] ──── ödəniş kəsildikdə ──► [ Suspended ]
              │                                      │
         abunə bitdikdə                     ödəniş gəldikdə
              │                                      │
              ▼                                      │
         [ Expired ]                          [ Active ] ◄──┘
```

| Status | Login | Token Yenileme | Modullar |
|--------|-------|----------------|----------|
| `Active` | ✅ | ✅ | ✅ |
| `Trial` | ✅ | ✅ | ✅ |
| `Suspended` | ❌ (403) | ❌ | ❌ |
| `Expired` | ❌ (403) | ❌ | ❌ |

**Qayda:** `tenant_status` claim-i `Suspended` və ya `Expired` olduqda modul bütün sorğuları rədd etməlidir.

---

## 5. ICurrentTenantService Pattern

Hər modulda **bu interfeysi** implement et — bütün servislər buradan tenant kontekstini alır:

```csharp
// YourModule/Application/Interfaces/ICurrentTenantService.cs
public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
    string? TenantStatus { get; }
    bool IsAuthenticated { get; }
    bool IsPlatformSuperAdmin { get; }
    bool IsTenantAdmin { get; }
}
```

### Implementasiya (Auth Service-dəki ilə eyni məntiqdə):

```csharp
// YourModule/Infrastructure/Services/CurrentTenantService.cs
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    // JWT-dəki "tenant_id" claim-ini oxu
    public Guid? TenantId
    {
        get
        {
            var val = _httpContextAccessor.HttpContext?.User
                          ?.FindFirstValue("tenant_id");
            return Guid.TryParse(val, out var id) ? id : null;
        }
    }

    // JWT-dəki "sub" və ya "nameid" claim-ini oxu
    public Guid? UserId
    {
        get
        {
            var val = _httpContextAccessor.HttpContext?.User
                          ?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? _httpContextAccessor.HttpContext?.User
                          ?.FindFirstValue("sub");
            return Guid.TryParse(val, out var id) ? id : null;
        }
    }

    public string? TenantStatus =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_status");

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public bool IsPlatformSuperAdmin =>
        _httpContextAccessor.HttpContext?.User?.IsInRole("PlatformSuperAdmin") == true;

    public bool IsTenantAdmin =>
        _httpContextAccessor.HttpContext?.User?.IsInRole("TenantAdmin") == true;
}
```

### DI Qeydiyyatı:

```csharp
// Program.cs
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
```

---

## 6. Tenant İzolyasiyası — Əsas Qayda

**Hər entity-də `TenantId` olmalıdır. Hər sorğuda `TenantId` filter tətbiq edilməlidir.**

### Entity nümunəsi:

```csharp
// YourModule/Domain/Entities/Contact.cs
public class Contact : BaseEntity
{
    public Guid TenantId { get; set; }      // ← MÜTLƏQ LAZIM

    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    // ...
}

// BaseEntity (Auth Service ilə eyni pattern):
public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}
```

### Repository nümunəsi — tenant filtrini HƏMİŞƏ tətbiq et:

```csharp
// YourModule/Infrastructure/Repositories/ContactRepository.cs
public class ContactRepository : IContactRepository
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public ContactRepository(AppDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    // YANLIŞ — tenant filtri yoxdur:
    // public async Task<List<Contact>> GetAllAsync()
    //     => await _db.Contacts.ToListAsync();   ← BÜTÜN TENANTLARın məlumatı gəlir!

    // DÜZGÜN — tenant filtri var:
    public async Task<List<Contact>> GetAllAsync()
    {
        var tenantId = _tenant.TenantId
            ?? throw new UnauthorizedException("Tenant konteksti tapılmadı.");

        return await _db.Contacts
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)  // ← HƏMİŞƏ
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Contact?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenant.TenantId
            ?? throw new UnauthorizedException("Tenant konteksti tapılmadı.");

        // TenantId yoxlaması — başqa tenantin məlumatı gəlməsin
        return await _db.Contacts
            .FirstOrDefaultAsync(c => c.Id == id
                                   && c.TenantId == tenantId
                                   && !c.IsDeleted);
    }

    public async Task<Contact> AddAsync(Contact contact)
    {
        var tenantId = _tenant.TenantId
            ?? throw new UnauthorizedException("Tenant konteksti tapılmadı.");

        contact.TenantId = tenantId;  // ← Yaradarkən avtomatik set et
        contact.CreatedAt = DateTime.UtcNow;
        _db.Contacts.Add(contact);
        return contact;
    }
}
```

---

## 7. Service Qatında Tenant Məntiqi

```csharp
// YourModule/Application/Services/ContactService.cs
public class ContactService : IContactService
{
    private readonly IContactRepository _contactRepo;
    private readonly ICurrentTenantService _tenantService;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        IContactRepository contactRepo,
        ICurrentTenantService tenantService,
        ILogger<ContactService> logger)
    {
        _contactRepo = contactRepo;
        _tenantService = tenantService;
        _logger = logger;
    }

    // Tenant ID lazım olduqda bu helper istifadə et
    private Guid GetRequiredTenantId()
    {
        if (!_tenantService.TenantId.HasValue)
        {
            _logger.LogWarning("Tenant konteksti tapılmadı.");
            throw new UnauthorizedException("Tenant konteksti tapılmadı.");
        }
        return _tenantService.TenantId.Value;
    }

    // Tenant statusu yoxla — Suspended/Expired olduqda əməliyyata icazə vermə
    private void EnsureTenantIsActive()
    {
        var status = _tenantService.TenantStatus;
        if (status == "Suspended" || status == "Expired")
        {
            throw new TenantSuspendedException(
                "Şirkətin hesabı aktivdir deyil. Zəhmət olmasa platforma administratoru ilə əlaqə saxlayın.");
        }
    }

    public async Task<ContactResponse> CreateContactAsync(CreateContactRequest request)
    {
        var tenantId = GetRequiredTenantId();
        EnsureTenantIsActive();

        _logger.LogInformation("Kontakt yaradılır: TenantId={TenantId}", tenantId);

        var contact = new Contact
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone
            // TenantId → Repository.AddAsync içində avtomatik set edilir
        };

        await _contactRepo.AddAsync(contact);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(contact);
    }
}
```

---

## 8. Tenant Status Middleware (tövsiyə olunur)

Bütün API sorğularında tenant statusunu yoxlayan middleware:

```csharp
// YourModule/Middleware/TenantStatusMiddleware.cs
public class TenantStatusMiddleware
{
    private readonly RequestDelegate _next;

    public TenantStatusMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
    {
        // Yalnız authenticate olmuş sorğularda yoxla
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var status = tenantService.TenantStatus;

            if (status == "Suspended")
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Şirkətin hesabı dondurulub. Platforma administratoru ilə əlaqə saxlayın.",
                    code  = "TENANT_SUSPENDED"
                });
                return;
            }

            if (status == "Expired")
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Şirkətin abunəlik müddəti bitib.",
                    code  = "TENANT_EXPIRED"
                });
                return;
            }
        }

        await _next(context);
    }
}
```

```csharp
// Program.cs — sıralamanın DÜZGÜNlüyü vacibdir:
app.UseAuthentication();          // 1. Token doğrula
app.UseAuthorization();           // 2. Yetki yoxla
app.UseMiddleware<TenantStatusMiddleware>();  // 3. Tenant status yoxla
app.MapControllers();             // 4. Controller-lərə keç
```

---

## 9. İcazə (Permission) Sistemi

Permissionlar **modul kodunun prefiksi** ilə adlandırılır:

```
Format:   {module_code}.{resource}.{action}

Nümunələr:
  crm.contacts.view
  crm.contacts.create
  crm.contacts.update
  crm.contacts.delete
  hr.employees.view
  hr.payroll.approve
  inventory.items.view
  inventory.transfers.approve
```

### Modulda Permission yoxlaması:

```csharp
// Program.cs — Policy əlavəsi
builder.Services.AddAuthorization(options =>
{
    // Modul girişi
    options.AddPolicy("CrmAccess",
        p => p.RequireClaim("modules", "CRM"));

    // CRUD icazələr
    options.AddPolicy("CanViewContacts",
        p => p.RequireClaim("permissions", "crm.contacts.view"));
    options.AddPolicy("CanCreateContacts",
        p => p.RequireClaim("permissions", "crm.contacts.create"));
    options.AddPolicy("CanUpdateContacts",
        p => p.RequireClaim("permissions", "crm.contacts.update"));
    options.AddPolicy("CanDeleteContacts",
        p => p.RequireClaim("permissions", "crm.contacts.delete"));
});
```

```csharp
// Controller-də istifadə
[Authorize(Policy = "CrmAccess")]         // Modul abunəliyi yoxla
[ApiController]
[Route("api/crm/contacts")]
public class ContactsController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CanViewContacts")]
    public async Task<IActionResult> GetAll() { ... }

    [HttpPost]
    [Authorize(Policy = "CanCreateContacts")]
    public async Task<IActionResult> Create([FromBody] CreateContactRequest request) { ... }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanUpdateContacts")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactRequest request) { ... }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanDeleteContacts")]
    public async Task<IActionResult> Delete(Guid id) { ... }
}
```

### Extension metodlar (Claims-dən oxumaq üçün):

```csharp
// YourModule/Extensions/ClaimsPrincipalExtensions.cs
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? user.FindFirstValue("sub")
                      ?? throw new InvalidOperationException("UserId claim tapılmadı."));

    public static Guid GetTenantId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue("tenant_id")
                      ?? throw new InvalidOperationException("TenantId claim tapılmadı."));

    public static string GetTenantStatus(this ClaimsPrincipal user)
        => user.FindFirstValue("tenant_status") ?? "Unknown";

    public static bool HasPermission(this ClaimsPrincipal user, string permissionCode)
        => user.FindAll("permissions").Any(c => c.Value == permissionCode);

    public static bool HasModule(this ClaimsPrincipal user, string moduleCode)
        => user.FindAll("modules").Any(c => c.Value == moduleCode);

    public static List<string> GetRoles(this ClaimsPrincipal user)
        => user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
}
```

---

## 10. JWT Doğrulaması Konfiqurasiyası

```csharp
// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            // Bu üç dəyər Auth Service ilə EYNI olmalıdır:
            ValidIssuer   = "AltensorAuthService",
            ValidAudience = "AltensorPlatform",
            ValidateIssuerSigningKey = true,  // RSA public key ilə yoxla

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Auth Service-in JWKS endpoint-i (RSA Public Key)
        // Hər 24 saatda bir yenilənir (key rotation üçün)
        options.ConfigurationManager =
            new ConfigurationManager<OpenIdConnectConfiguration>(
                builder.Configuration["AuthService:JwksUrl"]!,
                new OpenIdConnectConfigurationRetriever(),
                TimeSpan.FromHours(24));
    });
```

```json
// appsettings.json
{
  "AuthService": {
    "JwksUrl": "http://localhost:5000/.well-known/jwks.json"
  }
}
```

---

## 11. Integration Event — UserCreated Webhook

Auth Service yeni istifadəçi yaradanda bu modulun endpoint-inə `HTTP POST` göndərir.

**Modulun implement etməli olduğu endpoint:**

```csharp
// YourModule/Controllers/Internal/AuthWebhookController.cs
[ApiController]
[Route("internal/webhooks")]
public class AuthWebhookController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public AuthWebhookController(IUserProfileService userProfileService)
        => _userProfileService = userProfileService;

    /// <summary>
    /// Auth Service buraya POST edir — yeni user yaradıldı.
    /// Auth Service appsettings.json → ModuleEndpoints → bu modulun URL-i qeydiyyata alınmalıdır.
    /// </summary>
    [HttpPost("user-created")]
    public async Task<IActionResult> OnUserCreated([FromBody] UserCreatedWebhookPayload payload)
    {
        // Modulun öz DB-sında user profil yarat
        await _userProfileService.EnsureProfileExistsAsync(
            payload.UserId,
            payload.TenantId,
            payload.Email,
            payload.FullName);

        return Ok();
    }
}

// Webhook payload strukturu (Auth Service-dəki UserCreatedIntegrationEvent ilə eyni):
public class UserCreatedWebhookPayload
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = default!;
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Auth Service-də bu modulun URL-ini qeyd et** (`appsettings.json`):

```json
// AltensorAuthService/appsettings.json
{
  "ModuleEndpoints": {
    "crm": "http://localhost:5200",
    "hr":  "http://localhost:5300"
  }
}
```

> `Module.Code` kiçik hərflə (məs. `"crm"`) ilə açar adı uyğun olmalıdır.
> Auth Service `POST http://localhost:5200/internal/webhooks/user-created` çağırır.

---

## 12. EF Core-da Tenant Filter (Global Query Filter)

`DbContext`-də **global query filter** istifadə etmək tövsiyə olunur — hər sorğuya avtomatik `TenantId` filtri əlavə edilir:

```csharp
// YourModule/Infrastructure/Persistence/AppDbContext.cs
public class AppDbContext : DbContext
{
    private readonly ICurrentTenantService _tenantService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Contact> Contacts { get; set; }
    // ... digər DbSet-lər

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // GLOBAL QUERY FILTER — bütün sorğulara avtomatik TenantId filtri
        modelBuilder.Entity<Contact>()
            .HasQueryFilter(c =>
                c.TenantId == _tenantService.TenantId!.Value
                && !c.IsDeleted);

        // Həmçinin soft-delete filter
        // Bu sayədə repository-də Where(c => c.TenantId == ...) yazmağa ehtiyac qalmır
    }
}
```

> **Qeyd:** Global Query Filter güclü vasitədir, lakin `IgnoreQueryFilters()` çağırılan yerlərdə işləmir.
> Kritik əməliyyatlarda (məs. Cross-tenant admin operasiyaları) ehtiyatlı ol.

---

## 13. Tam Program.cs Nümunəsi

```csharp
// YourModule/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Auth Service JWT ──────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidIssuer              = "AltensorAuthService",
            ValidAudience            = "AltensorPlatform",
            ValidateIssuerSigningKey = true,
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.FromSeconds(30)
        };
        options.ConfigurationManager =
            new ConfigurationManager<OpenIdConnectConfiguration>(
                builder.Configuration["AuthService:JwksUrl"]!,
                new OpenIdConnectConfigurationRetriever(),
                TimeSpan.FromHours(24));
    });

// ── 2. Authorization Policies ────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ModuleAccess",        p => p.RequireClaim("modules",      "CRM"));
    options.AddPolicy("CanViewContacts",     p => p.RequireClaim("permissions",  "crm.contacts.view"));
    options.AddPolicy("CanCreateContacts",   p => p.RequireClaim("permissions",  "crm.contacts.create"));
    options.AddPolicy("CanUpdateContacts",   p => p.RequireClaim("permissions",  "crm.contacts.update"));
    options.AddPolicy("CanDeleteContacts",   p => p.RequireClaim("permissions",  "crm.contacts.delete"));
});

// ── 3. Infrastructure ────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 4. Application Services ──────────────────────────────────────
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IContactService, ContactService>();

builder.Services.AddControllers();

var app = builder.Build();

// ── 5. Middleware sıralaması — DƏYİŞDİRMƏ ──────────────────────
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantStatusMiddleware>();   // Suspended/Expired yoxlaması
app.MapControllers();

app.Run();
```

---

## 14. Rol Sistemi

Auth Service-dəki rollar:

| Rol | Scope | Nə edə bilər |
|-----|-------|--------------|
| `PlatformSuperAdmin` | Platform-wide | Bütün tenantları, modulları, abunəlikləri idarə edir |
| `TenantAdmin` | Tenant-scoped | Öz tenantının user-lərini, rollarını idarə edir |
| `[Xüsusi rollar]` | Tenant-scoped | TenantAdmin tərəfindən yaradılır, müəyyən permissionlar verilir |

### Modulda rol yoxlaması:

```csharp
// TenantAdmin VƏ PlatformSuperAdmin hər ikisi bu endpoint-ə daxil ola bilər:
[Authorize(Roles = "TenantAdmin,PlatformSuperAdmin")]
[HttpPost("settings")]
public IActionResult UpdateSettings() { ... }

// Yalnız PlatformSuperAdmin:
[Authorize(Roles = "PlatformSuperAdmin")]
[HttpDelete("tenant/{id}")]
public IActionResult DeleteTenant(Guid id) { ... }
```

---

## 15. İstisna (Exception) Sinifləri

Bu istisnaları yaradıb middleware-ə qoşun (Auth Service ilə eyni pattern):

```csharp
// Exceptions/UnauthorizedException.cs
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

// Exceptions/TenantSuspendedException.cs
public class TenantSuspendedException : Exception
{
    public TenantSuspendedException(string message) : base(message) { }
}

// Exceptions/NotFoundException.cs
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

// Exceptions/ValidationException.cs
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

```csharp
// Middleware/GlobalExceptionMiddleware.cs
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedException ex)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (TenantSuspendedException ex)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message, code = "TENANT_SUSPENDED" });
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gözlənilməz xəta baş verdi.");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Daxili server xətası." });
        }
    }
}
```

---

## 16. Yeni Modul Yaratmaq — Addım-addım Siyahı

Agent bu addımları ardıcıl icra etməlidir:

- [ ] **A. Layihə strukturu**
  - [ ] `Domain/Entities/` — modulun entity-ləri, hamısına `TenantId` əlavə et, `BaseEntity`-dən inherit et
  - [ ] `Domain/Repositories/` — interface-lər
  - [ ] `Application/Interfaces/` — `ICurrentTenantService` + servis interface-ləri
  - [ ] `Application/Services/` — `CurrentTenantService` + biznes servisləri
  - [ ] `Infrastructure/Persistence/AppDbContext.cs` — EF Core, global query filter
  - [ ] `Infrastructure/Repositories/` — implementasiyalar
  - [ ] `Presentation/Controllers/` — API controller-lər
  - [ ] `Middleware/` — `TenantStatusMiddleware`, `GlobalExceptionMiddleware`

- [ ] **B. JWT inteqrasiyası**
  - [ ] `Microsoft.AspNetCore.Authentication.JwtBearer` NuGet paketi
  - [ ] `Microsoft.IdentityModel.Protocols.OpenIdConnect` NuGet paketi
  - [ ] `Program.cs`-ə JWT konfiqurasiyası (Issuer = `"AltensorAuthService"`, Audience = `"AltensorPlatform"`)
  - [ ] `appsettings.json`-a `AuthService:JwksUrl` əlavə et

- [ ] **C. Tenant izolyasiyası**
  - [ ] `ICurrentTenantService` implement edilib
  - [ ] Bütün entity-lərdə `TenantId : Guid` var
  - [ ] Bütün repository sorğularında `TenantId` filter var
  - [ ] `TenantStatusMiddleware` `Program.cs`-ə əlavə edilib

- [ ] **D. Permission sistemi**
  - [ ] Permission kodları `{module}.{resource}.{action}` formatında
  - [ ] Authorization Policy-lər `Program.cs`-ə əlavə edilib
  - [ ] Controller-lər `[Authorize(Policy="...")]` ilə qorunur

- [ ] **E. Webhook**
  - [ ] `POST /internal/webhooks/user-created` endpoint implement edilib
  - [ ] Auth Service-in `appsettings.json`-da `ModuleEndpoints` bölməsinə bu modulun URL-i əlavə edilib

- [ ] **F. Yoxlama**
  - [ ] Auth Service çalışır (`http://localhost:5000`)
  - [ ] JWKS endpoint əlçatanlıdır (`http://localhost:5000/.well-known/jwks.json`)
  - [ ] Login → token al → modula sorğu göndər → 200 OK

---

## Qısa Xülasə (Agent üçün)

```
1. JWT oxu → tenant_id claim-ini al → bu tenantın məlumatlarını göstər
2. Hər DB sorğusunda WHERE TenantId = {tenantId} əlavə et
3. tenant_status = Suspended/Expired isə → 403 qaytar
4. permissions claim-i → API girişini idarə et
5. modules claim-i → modul abunəliyini yoxla
6. POST /internal/webhooks/user-created → user sinxronizasiyası
```

---

*Sənəd AltensorAuthService v1.0 arxitekturası əsasında hazırlanmışdır.*
*Bu sənədi agentə ver — agent lazım olan hər şeyi buradan götürə bilər.*
