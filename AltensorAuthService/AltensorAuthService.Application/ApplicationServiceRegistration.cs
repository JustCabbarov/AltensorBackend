using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Application.Services;
using AltensorAuthService.Contract.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AltensorAuthService.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ICurrentTenantService, CurrentTenantService>();
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITenantManagementService, TenantManagementService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<IRoleManagementService, RoleManagementService>();
            services.AddScoped<IEmailSender, EmailSenderService>();
            services.AddScoped<IPasswordResetService, PasswordResetService>();

            return services;
        }
    }
}

