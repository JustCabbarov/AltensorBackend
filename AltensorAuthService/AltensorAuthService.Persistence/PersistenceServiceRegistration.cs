using AltensorAuthService.Contract.Events;
using AltensorAuthService.Domain.Repositories;
using AltensorAuthService.Persistence.Data;
using AltensorAuthService.Persistence.Integration;
using AltensorAuthService.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AltensorAuthService.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            // Unit Of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Specific Repositories (Composition)
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<ITenantModuleSubscriptionRepository, TenantModuleSubscriptionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IPasswordResetOtpRepository, PasswordResetOtpRepository>();

            // Integration Event Publisher (HTTP-based)
            // Gələcəkdə RabbitMQ/MassTransit istifadə etmək üçün yalnız bu hissəni dəyişmək kifayətdir.
            services.AddHttpClient("ModuleInternal", (sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var secret = config["Webhook:SharedSecret"] 
                          ?? config["InternalCommunication:ApiKey"];
                if (!string.IsNullOrWhiteSpace(secret))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Webhook-Secret", secret);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Internal-Api-Key", secret);
                }
                client.Timeout = TimeSpan.FromSeconds(5);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
            services.AddScoped<ModuleEndpointRegistry>();
            services.AddScoped<IIntegrationEventPublisher, HttpIntegrationEventPublisher>();

            return services;
        }
    }
}
