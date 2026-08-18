namespace AltensorAuthService.Contract.Services
{
    public interface IPasswordResetService
    {
        Task ForgotPasswordAsync(string email, string tenantSlug);
        Task ResetPasswordAsync(string email, string tenantSlug, string otp, string newPassword);
    }
}
