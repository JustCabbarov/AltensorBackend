using AltensorAuthService.Presentation.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AltensorAuthService.Presentation.Extensions
{
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddAppAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

            services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("PlatformSuperAdmin", policy => policy.RequireRole("PlatformSuperAdmin"));
                options.AddPolicy("TenantAdmin", policy => policy.RequireRole("TenantAdmin"));
            });

            return services;
        }
    }
}
