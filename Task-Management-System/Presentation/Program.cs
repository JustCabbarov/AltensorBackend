using Application.Profiles;
using Application.Services;
using ApplicationLayer.Services;
using Contract.Services;
using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using Persistence.Data;
using Persistence.Repositories;
using Presentation.ExceptionHandler;
using Presentation.Extensions;
using Presentation.Hubs;
using Presentation.Middleware;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

namespace Presentationn
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            var builder = WebApplication.CreateBuilder(args);

            // ================= Controllers =================
            builder.Services.AddControllers();

            // ================= DbContext =================
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")));

            // ================= Serilog =================
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            // ================= Tenant Servisi =================
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

            // ================= DI =================
            builder.Services.AddScoped<IUnityOfWork, UnityOfWork>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
            builder.Services.AddScoped<IAuthorizeService, AuthorizationService>();
            builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<ITaksService, TaskService>();
            builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
            builder.Services.AddScoped<ITaskAttachmentService, TaskAttachmentService>();
            builder.Services.AddScoped<IPerformanceService, PerformanceService>();
            builder.Services.AddScoped<IWorkGroupService, WorkGroupService>();

            builder.Services.AddScoped<IEmailSender, EmailSender>();

            builder.Services.AddSignalR();

            // ================= Exception Handlers =================
            builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
            builder.Services.AddExceptionHandler<NullExceptionHandler>();
            builder.Services.AddExceptionHandler<UnauthorizedExceptionHandler>();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // ================= AutoMapper =================
            builder.Services.AddAutoMapper(m => m.AddProfile(new CustomProfile()));

            // ================= Authentication (RS256 + JWKS Key Resolver) =================
            builder.Services.AddAltensorAuthentication(builder.Configuration);

            // ================= Authorization Policies =================
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.AddPolicy("CanViewTasks", p => p.RequireClaim("permissions", "tms.tasks.view"));
                options.AddPolicy("CanCreateTasks", p => p.RequireClaim("permissions", "tms.tasks.create"));
                options.AddPolicy("CanUpdateTasks", p => p.RequireClaim("permissions", "tms.tasks.update"));
                options.AddPolicy("CanDeleteTasks", p => p.RequireClaim("permissions", "tms.tasks.delete"));

                options.AddPolicy("CanViewWorkGroups", p => p.RequireClaim("permissions", "tms.workgroups.view"));
                options.AddPolicy("CanManageWorkGroups", p => p.RequireClaim("permissions", "tms.workgroups.manage"));

                options.AddPolicy("CanViewPerformance", p => p.RequireClaim("permissions", "tms.performance.view"));
                options.AddPolicy("TMSAccess", p => p.RequireClaim("modules", "TMS"));
            });

            // ================= Minio & Swagger =================
            builder.Services.AddSingleton<IMinioClient>(sp =>
            {
                var config = builder.Configuration.GetSection("Minio");
                return new MinioClient()
                    .WithEndpoint(config["Endpoint"])
                    .WithCredentials(config["AccessKey"], config["SecretKey"])
                    .Build();
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Task Management API",
                    Version = "v1",
                    Description = "Altensor Platform — Task Management Module (Multi-Tenant)"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Bearer {token} — Auth Service-dən alınan JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ClientPermission", policy =>
                {
                    policy
                        .SetIsOriginAllowed(origin => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var app = builder.Build();

            var forwardedOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedOptions.KnownNetworks.Clear();
            forwardedOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedOptions);

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseExceptionHandler();

            app.UseCors("ClientPermission");

            // Nginx HTTPS-i idarə etdiyi üçün app.UseHttpsRedirection() daxildə lazım deyil (şərhə alırıq)
            // app.UseHttpsRedirection();

            // Pipeline sıralaması
            app.UseAuthentication();
            app.UseMiddleware<TenantStatusMiddleware>();
            app.UseAuthorization();

            app.MapControllers();
            app.MapGet("/", () => Results.Ok(new { status = "Altensor TMS API Running", time = DateTime.UtcNow })).AllowAnonymous();
            app.MapHub<NotificationHub>("/hubs/notification");

            // ================= Auto Create DB Tables =================
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }

            await app.RunAsync();
        }
    }
}
