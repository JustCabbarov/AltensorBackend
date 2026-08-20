using System.Text.Json.Serialization;
using Altensorcrm.Api.Extensions;
using Altensorcrm.Api.Middlewares;
using Altensorcrm.Application.Extentions;
using Altensorcrm.Application.Profiles;
using Altensorcrm.Contract.Options;
using Altensorcrm.Contract.Services.Tenant;
using Altensorcrm.Contract.Services.User;
using Altensorcrm.Infrastructure.Services;
using Altensorcrm.Persistence.Data;
using Altensorcrm.Persistence.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// ── 1. Controllers & JSON Options ─────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ── 2. AutoMapper ─────────────────────────────────────────────
builder.Services.AddAutoMapper(_ => { }, typeof(CustomProfile).Assembly);

// ── 3. Options & Configuration ────────────────────────────────
builder.Services.Configure<EmailOption>(builder.Configuration.GetSection("Email"));

// ── 4. Multi-Tenant & User Infrastructure DI ──────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentTenantService>();
builder.Services.AddScoped<ICurrentTenantService>(sp => sp.GetRequiredService<CurrentTenantService>());
builder.Services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<CurrentTenantService>());

// ── 5. Application & Persistence Services ─────────────────────
builder.Services.AddServiceRegistration();
builder.Services.AddPersistenceServices(builder.Configuration);

// ── 6. JWT Authentication with AltensorAuthService (JWKS) ────
builder.Services.AddAltensorAuthentication(builder.Configuration);

// ── 7. Authorization Policies ─────────────────────────────────
static bool HasPermissionOrAdmin(Microsoft.AspNetCore.Authorization.AuthorizationHandlerContext ctx, string permission, string fallbackPerm = "")
{
    if (ctx.User.IsInRole("TenantAdmin") || ctx.User.IsInRole("PlatformSuperAdmin") || ctx.User.IsInRole("Admin"))
        return true;

    if (ctx.User.HasClaim("permissions", permission))
        return true;

    if (!string.IsNullOrEmpty(fallbackPerm) && ctx.User.HasClaim("permissions", fallbackPerm))
        return true;

    return false;
}

builder.Services.AddAuthorization(options =>
{
    // Module subscription policy
    options.AddPolicy("CrmModuleAccess", p => p.RequireAssertion(ctx => 
        ctx.User.HasClaim("modules", "CRM") || ctx.User.HasClaim("modules", "crm")));

    // Contacts policies
    options.AddPolicy("CanViewContacts", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.contacts.view", "crm.read")));
    options.AddPolicy("CanCreateContacts", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.contacts.create", "crm.write")));
    options.AddPolicy("CanUpdateContacts", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.contacts.update", "crm.write")));
    options.AddPolicy("CanDeleteContacts", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.contacts.delete", "crm.delete")));

    // Leads policies
    options.AddPolicy("CanViewLeads", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.leads.view", "crm.read")));
    options.AddPolicy("CanCreateLeads", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.leads.create", "crm.write")));
    options.AddPolicy("CanUpdateLeads", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.leads.update", "crm.write")));
    options.AddPolicy("CanDeleteLeads", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.leads.delete", "crm.delete")));

    // Deals policies
    options.AddPolicy("CanViewDeals", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.deals.view", "crm.read")));
    options.AddPolicy("CanCreateDeals", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.deals.create", "crm.write")));
    options.AddPolicy("CanUpdateDeals", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.deals.update", "crm.write")));
    options.AddPolicy("CanDeleteDeals", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.deals.delete", "crm.delete")));

    // Organizations policies
    options.AddPolicy("CanViewOrganizations", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.organizations.view", "crm.read")));
    options.AddPolicy("CanCreateOrganizations", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.organizations.create", "crm.write")));
    options.AddPolicy("CanUpdateOrganizations", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.organizations.update", "crm.write")));
    options.AddPolicy("CanDeleteOrganizations", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.organizations.delete", "crm.delete")));

    // Products policies
    options.AddPolicy("CanViewProducts", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.products.view", "crm.read")));
    options.AddPolicy("CanCreateProducts", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.products.create", "crm.write")));
    options.AddPolicy("CanUpdateProducts", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.products.update", "crm.write")));
    options.AddPolicy("CanDeleteProducts", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.products.delete", "crm.delete")));

    // Tasks policies
    options.AddPolicy("CanViewTasks", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.tasks.view", "crm.read")));
    options.AddPolicy("CanCreateTasks", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.tasks.create", "crm.write")));
    options.AddPolicy("CanUpdateTasks", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.tasks.update", "crm.write")));
    options.AddPolicy("CanDeleteTasks", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.tasks.delete", "crm.delete")));

    // Notes policies
    options.AddPolicy("CanViewNotes", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.notes.view", "crm.read")));
    options.AddPolicy("CanCreateNotes", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.notes.create", "crm.write")));
    options.AddPolicy("CanUpdateNotes", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.notes.update", "crm.write")));
    options.AddPolicy("CanDeleteNotes", p => p.RequireAssertion(ctx => HasPermissionOrAdmin(ctx, "crm.notes.delete", "crm.delete")));

    // Settings policy
    options.AddPolicy("CanManageSettings", p => p.RequireAssertion(ctx => 
        ctx.User.IsInRole("TenantAdmin") || ctx.User.IsInRole("PlatformSuperAdmin") || ctx.User.IsInRole("Admin") || ctx.User.HasClaim("permissions", "crm.settings.manage")));
});

// ── 8. CORS ───────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── 9. Swagger / OpenAPI ──────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AltensorCRM API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token issued by AltensorAuthService",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// ── 10. Database Migration ────────────────────────────────────
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"[DB] Migration warning: {ex.Message}");
}

// ── 11. Middleware Pipeline ───────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();


    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AltensorCRM API v1");
    });


var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseMiddleware<TenantStatusMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
