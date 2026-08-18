using AltensorAuthService.Contract.Auth;
using AltensorAuthService.Domain.Entities;

namespace AltensorAuthService.Application.Interfaces
{
    public interface IJwtTokenService
    {
        Task<string> GenerateAccessTokenAsync(
            ApplicationUser user,
            string tenantStatus,
            IEnumerable<string> roles,
            IEnumerable<string> permissions,
            IEnumerable<string> modules);

        string GenerateRawRefreshToken();
        string HashRefreshToken(string rawRefreshToken);
        JwksDto GetJwks();
    }
}
