using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Altensorcrm.Api.Extensions;

public static class AuthenticationExtensions
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SecurityKey> KeyCache = new();
    private static DateTime _lastKeyFetch = DateTime.MinValue;
    private static readonly object KeyLock = new();

    public static IServiceCollection AddAltensorAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "AltensorAuthService";
        var audience = configuration["Jwt:Audience"] ?? "AltensorPlatform";
        var jwksUrl = configuration["AuthService:JwksEndpoint"] 
                   ?? configuration["AuthService:JwksUrl"]
                   ?? "https://api-info.altensor.com/.well-known/jwks.json";
        var requireHttpsMetadata = configuration.GetValue<bool>("Jwt:RequireHttpsMetadata", true);

        var httpHandler = new HttpClientHandler();
        // Allow self-signed certs in development environment if needed
        var isDev = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);
        if (isDev)
        {
            httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        var jwksHttpClient = new HttpClient(httpHandler) { Timeout = TimeSpan.FromSeconds(10) };

        System.Collections.Generic.IList<SecurityKey> GetSigningKeys(string? kid)
        {
            lock (KeyLock)
            {
                if (KeyCache.Count > 0 && (DateTime.UtcNow - _lastKeyFetch < TimeSpan.FromMinutes(15)))
                {
                    if (string.IsNullOrEmpty(kid) || KeyCache.ContainsKey(kid))
                    {
                        return KeyCache.Values.ToList();
                    }
                }

                try
                {
                    var response = jwksHttpClient.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
                    var jwks = new JsonWebKeySet(response);
                    var keys = jwks.GetSigningKeys();

                    if (keys != null && keys.Count > 0)
                    {
                        KeyCache.Clear();
                        foreach (var key in keys)
                        {
                            if (!string.IsNullOrEmpty(key.KeyId))
                            {
                                KeyCache[key.KeyId] = key;
                            }
                        }
                        _lastKeyFetch = DateTime.UtcNow;
                        return keys;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Auth] JWKS açarları oxunarkən xəta: {ex.Message}");
                }

                return KeyCache.Values.ToList();
            }
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = requireHttpsMetadata;
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

                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                {
                    return GetSigningKeys(kid);
                }
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"[JWT] Auth failed: {context.Exception.Message}");
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
