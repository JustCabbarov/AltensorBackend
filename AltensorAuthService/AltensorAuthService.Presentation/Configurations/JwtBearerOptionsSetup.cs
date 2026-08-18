using AltensorAuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AltensorAuthService.Presentation.Configurations
{
    public class JwtBearerOptionsSetup : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtBearerOptionsSetup> _logger;

        public JwtBearerOptionsSetup(
            IJwtTokenService jwtTokenService,
            IConfiguration configuration,
            ILogger<JwtBearerOptionsSetup> logger)
        {
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
            _logger = logger;
        }

        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "AltensorAuthService";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "AltensorPlatform";

            _logger.LogInformation("JwtBearerOptions konfiqurasiya edilir: Issuer='{Issuer}', Audience='{Audience}'", jwtIssuer, jwtAudience);

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                ValidateIssuerSigningKey = true,
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                {
                    _logger.LogDebug("JWT imza açarı tələb olunur (kid: {Kid})", kid);

                    var jwks = _jwtTokenService.GetJwks();
                    var matchingKey = jwks.Keys.FirstOrDefault(k => k.Kid == kid) ?? jwks.Keys.FirstOrDefault();

                    if (matchingKey != null)
                    {
                        var rsaParams = new RSAParameters
                        {
                            Modulus = Base64UrlEncoder.DecodeBytes(matchingKey.N),
                            Exponent = Base64UrlEncoder.DecodeBytes(matchingKey.E)
                        };
                        var rsa = RSA.Create();
                        rsa.ImportParameters(rsaParams);
                        return new[] { new RsaSecurityKey(rsa) { KeyId = matchingKey.Kid } };
                    }

                    _logger.LogWarning("JWT üçün uyğun imza açarı tapılmadı (kid: {Kid})", kid);
                    return Enumerable.Empty<SecurityKey>();
                }
            };
        }
    }
}
