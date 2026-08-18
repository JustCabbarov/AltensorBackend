using AltensorAuthService.Contract.Auth;

namespace AltensorAuthService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(LoginRequest request);
        Task<TokenResponse> RefreshAsync(RefreshRequest request);
        Task LogoutAsync(string? rawRefreshToken);
        Task LogoutAllDevicesAsync(Guid userId);
        Task<UserInfoDto> GetCurrentUserInfoAsync();
    }
}
