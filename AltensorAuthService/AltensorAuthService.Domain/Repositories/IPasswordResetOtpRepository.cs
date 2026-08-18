using AltensorAuthService.Domain.Entities;

namespace AltensorAuthService.Domain.Repositories
{
    public interface IPasswordResetOtpRepository
    {
        Task AddAsync(PasswordResetOtp otp);
        Task<PasswordResetOtp?> GetValidOtpAsync(Guid userId, string code);
        Task MarkAsUsedAsync(PasswordResetOtp otp);
    }
}
