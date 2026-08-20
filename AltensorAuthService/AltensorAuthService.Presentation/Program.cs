using AltensorAuthService.Application;
using AltensorAuthService.Persistence;
using AltensorAuthService.Persistence.Data.Seed;
using AltensorAuthService.Presentation.Extensions;
using AltensorAuthService.Presentation.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

LoggingExtensions.ConfigureSerilog();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// 1. HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// 2. Persistence Layer (DbContext, Repositories, UnitOfWork)
builder.Services.AddPersistenceServices(builder.Configuration);

// 3. Application Layer (Services)
builder.Services.AddApplicationServices();

// 4. Identity
builder.Services.AddAppIdentity();

// 5. Authentication & Authorization
builder.Services.AddAppAuthentication();

// 6. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 7. Controllers
builder.Services.AddControllers();

// 8. Swagger
builder.Services.AddAppSwagger();

var app = builder.Build();

// Serilog HTTP Request Logging
app.UseSerilogRequestLogging();

// Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger UI
app.UseAppSwagger(app.Environment);
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseCors("AllowAll");


// Authentication & Authorization Pipeline
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed initial data
await DataSeeder.SeedAsync(app.Services);

await app.RunAsync();
