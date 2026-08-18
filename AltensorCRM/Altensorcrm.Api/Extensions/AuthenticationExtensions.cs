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

    public static IServiceCollection AddAltensorAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "AltensorAuthService";
        var audience = configuration["Jwt:Audience"] ?? "AltensorPlatform";
        var jwksUrl = configuration["AuthService:JwksEndpoint"] 
                   ?? configuration["AuthService:JwksUrl"]
                   ?? "https://localhost:7049/.well-known/jwks.json";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
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
                    // Check cache first
                    if (!string.IsNullOrEmpty(kid) && KeyCache.TryGetValue(kid, out var cachedKey))
                    {
                        return new[] { cachedKey };
                    }

                    try
                    {
                        var handler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };
                        using var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
                        
                        string response;
                        try
                        {
                            response = httpClient.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
                        }
                        catch
                        {
                            // Fallback to HTTP if HTTPS fails
                            var fallbackUrl = jwksUrl.Replace("https://localhost:7049", "http://localhost:5155");
                            response = httpClient.GetStringAsync(fallbackUrl).GetAwaiter().GetResult();
                        }

                        using var doc = JsonDocument.Parse(response);
                        if (doc.RootElement.TryGetProperty("keys", out var keys) || doc.RootElement.TryGetProperty("Keys", out keys))
                        {
                            var matchedKeys = new System.Collections.Generic.List<SecurityKey>();

                            foreach (var key in keys.EnumerateArray())
                            {
                                var currentKid = key.TryGetProperty("kid", out var kidProp) ? kidProp.GetString() : null;
                                var n = key.TryGetProperty("n", out var nProp) ? nProp.GetString() : null;
                                var e = key.TryGetProperty("e", out var eProp) ? eProp.GetString() : null;

                                if (!string.IsNullOrEmpty(n) && !string.IsNullOrEmpty(e))
                                {
                                    var rsaParams = new RSAParameters
                                    {
                                        Modulus = Base64UrlEncoder.DecodeBytes(n),
                                        Exponent = Base64UrlEncoder.DecodeBytes(e)
                                    };

                                    var rsa = RSA.Create();
                                    rsa.ImportParameters(rsaParams);
                                    var rsaKey = new RsaSecurityKey(rsa) { KeyId = currentKid ?? string.Empty };

                                    if (!string.IsNullOrEmpty(currentKid))
                                    {
                                        KeyCache[currentKid] = rsaKey;
                                    }

                                    if (string.IsNullOrEmpty(kid) || currentKid == kid)
                                    {
                                        matchedKeys.Add(rsaKey);
                                    }
                                }
                            }

                            if (matchedKeys.Count > 0)
                            {
                                return matchedKeys;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Auth] JWKS açarları oxunarkən xəta: {ex.Message}");
                    }

                    return KeyCache.Values;
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
