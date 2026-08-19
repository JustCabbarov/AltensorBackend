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
using Microsoft.OpenApi.Models;

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
builder.Services.AddAuthorization(options =>
{
    // Module subscription policy
    options.AddPolicy("CrmModuleAccess", p => p.RequireClaim("modules", "CRM"));

    // Contacts policies
    options.AddPolicy("CanViewContacts", p => p.RequireClaim("permissions", "crm.contacts.view"));
    options.AddPolicy("CanCreateContacts", p => p.RequireClaim("permissions", "crm.contacts.create"));
    options.AddPolicy("CanUpdateContacts", p => p.RequireClaim("permissions", "crm.contacts.update"));
    options.AddPolicy("CanDeleteContacts", p => p.RequireClaim("permissions", "crm.contacts.delete"));

    // Leads policies
    options.AddPolicy("CanViewLeads", p => p.RequireClaim("permissions", "crm.leads.view"));
    options.AddPolicy("CanCreateLeads", p => p.RequireClaim("permissions", "crm.leads.create"));
    options.AddPolicy("CanUpdateLeads", p => p.RequireClaim("permissions", "crm.leads.update"));
    options.AddPolicy("CanDeleteLeads", p => p.RequireClaim("permissions", "crm.leads.delete"));

    // Deals policies
    options.AddPolicy("CanViewDeals", p => p.RequireClaim("permissions", "crm.deals.view"));
    options.AddPolicy("CanCreateDeals", p => p.RequireClaim("permissions", "crm.deals.create"));
    options.AddPolicy("CanUpdateDeals", p => p.RequireClaim("permissions", "crm.deals.update"));
    options.AddPolicy("CanDeleteDeals", p => p.RequireClaim("permissions", "crm.deals.delete"));
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


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantStatusMiddleware>();

app.MapControllers();

app.Run();
