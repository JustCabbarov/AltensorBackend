using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Contract.Auth;
using AltensorAuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AltensorAuthService.Application.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtTokenService> _logger;
        private readonly RSA _rsa;
        private readonly RsaSecurityKey _signingKey;
        private readonly string _keyId;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpiryMinutes;

        public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _issuer = _configuration["Jwt:Issuer"] ?? "AltensorAuthService";
            _audience = _configuration["Jwt:Audience"] ?? "AltensorPlatform";
            _keyId = _configuration["Jwt:KeyId"] ?? "altensor-auth-key-1";

            if (!int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out _accessTokenExpiryMinutes))
            {
                _accessTokenExpiryMinutes = 15;
            }

            _rsa = RSA.Create();
            var privateKeyPem = _configuration["Jwt:PrivateKeyPem"];
            var keyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jwt_rsa_private.pem");

            if (!string.IsNullOrWhiteSpace(privateKeyPem))
            {
                try
                {
                    _rsa.ImportFromPem(privateKeyPem.AsSpan());
                    _logger.LogInformation("RSA açarı konfiqurasiyadakı PEM mətnindən uğurla yükləndi. KeyId='{KeyId}'", _keyId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "PEM açarını oxuyarkən xəta baş verdi. Avtomatik 2048-bit RSA açarı generasiya edilir.");
                    _rsa = RSA.Create(2048);
                }
            }
            else if (File.Exists(keyFilePath))
            {
                try
                {
                    var pemText = File.ReadAllText(keyFilePath);
                    _rsa.ImportFromPem(pemText.AsSpan());
                    _logger.LogInformation("RSA açarı fayldan ({FilePath}) uğurla yükləndi. KeyId='{KeyId}'", keyFilePath, _keyId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fayldakı PEM açarını oxuyarkən xəta baş verdi. Yeni RSA açarı generasiya edilir.");
                    _rsa = RSA.Create(2048);
                    try
                    {
                        File.WriteAllText(keyFilePath, _rsa.ExportPkcs8PrivateKeyPem());
                    }
                    catch { }
                }
            }
            else
            {
                _logger.LogInformation("Konfiqurasiyada PEM açarı tapılmadı. Yeni 2048-bit RSA açarı generasiya edildi və fayla yazıldı ({FilePath}). KeyId='{KeyId}'", keyFilePath, _keyId);
                _rsa = RSA.Create(2048);
                try
                {
                    File.WriteAllText(keyFilePath, _rsa.ExportPkcs8PrivateKeyPem());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RSA açarını fayla yazmaq mümkün olmadı.");
                }
            }

            _signingKey = new RsaSecurityKey(_rsa)
            {
                KeyId = _keyId
            };
        }

        public Task<string> GenerateAccessTokenAsync(
            ApplicationUser user,
            string tenantStatus,
            IEnumerable<string> roles,
            IEnumerable<string> permissions,
            IEnumerable<string> modules)
        {
            _logger.LogDebug("JWT Access Token generasiya edilir: UserId={UserId}, TenantId={TenantId}, TenantStatus='{TenantStatus}'",
                user.Id, user.TenantId, tenantStatus);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(AppClaimTypes.TenantId, user.TenantId.ToString()),
                new Claim("tenant_id", user.TenantId.ToString()),
                new Claim(AppClaimTypes.TenantStatus, tenantStatus),
                new Claim("tenant_status", tenantStatus)
            };

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.FullName));
            }

            // Add roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("roles", role));
            }

            // Add permissions
            foreach (var permission in permissions)
            {
                claims.Add(new Claim(AppClaimTypes.Permission, permission));
                claims.Add(new Claim("permissions", permission));
            }

            // Add modules
            foreach (var module in modules)
            {
                claims.Add(new Claim(AppClaimTypes.Module, module));
                claims.Add(new Claim("modules", module));
            }

            var signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogDebug("JWT Access Token uğurla yaradıldı: UserId={UserId}, KeyId='{KeyId}'", user.Id, _keyId);

            return Task.FromResult(tokenString);
        }

        public string GenerateRawRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public string HashRefreshToken(string rawRefreshToken)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawRefreshToken);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public JwksDto GetJwks()
        {
            _logger.LogDebug("JWKS publik açar dəsti sorğulandı.");

            var rsaParameters = _rsa.ExportParameters(false); // public key only

            var n = Base64UrlEncoder.Encode(rsaParameters.Modulus);
            var e = Base64UrlEncoder.Encode(rsaParameters.Exponent);

            return new JwksDto
            {
                Keys = new List<JwksKeyDto>
                {
                    new JwksKeyDto
                    {
                        Kty = "RSA",
                        Use = "sig",
                        Alg = "RS256",
                        Kid = _keyId,
                        N = n,
                        E = e
                    }
                }
            };
        }
    }
}
