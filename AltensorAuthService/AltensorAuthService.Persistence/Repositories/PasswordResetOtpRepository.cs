using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Repositories;
using AltensorAuthService.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AltensorAuthService.Persistence.Repositories
{
    public class PasswordResetOtpRepository : IPasswordResetOtpRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetOtpRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PasswordResetOtp otp)
        {
            await _context.PasswordResetOtps.AddAsync(otp);
        }

        public Task<PasswordResetOtp?> GetValidOtpAsync(Guid userId, string code)
        {
            return _context.PasswordResetOtps
                .Where(x => x.UserId == userId
                         && x.Code == code
                         && !x.IsUsed
                         && x.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public Task MarkAsUsedAsync(PasswordResetOtp otp)
        {
            otp.IsUsed = true;
            otp.UpdatedAt = DateTime.UtcNow;
            _context.PasswordResetOtps.Update(otp);
            return Task.CompletedTask;
        }
    }
}
