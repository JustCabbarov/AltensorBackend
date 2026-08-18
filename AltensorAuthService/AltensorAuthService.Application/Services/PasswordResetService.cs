using AltensorAuthService.Application.Exceptions;
using AltensorAuthService.Contract.Services;
using AltensorAuthService.Domain.Entities;
using AltensorAuthService.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AltensorAuthService.Application.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetOtpRepository _otpRepository;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            ITenantRepository tenantRepository,
            IUserRepository userRepository,
            IPasswordResetOtpRepository otpRepository,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<PasswordResetService> logger)
        {
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task ForgotPasswordAsync(string email, string tenantSlug)
        {
            _logger.LogInformation("Şifrə sıfırlama sorğusu: Email='{Email}', TenantSlug='{TenantSlug}'", email, tenantSlug);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(tenantSlug))
            {
                throw new ValidationException("Email və tenant slug sahələri tələb olunur.");
            }

            var tenant = await _tenantRepository.GetBySlugAsync(tenantSlug);
            if (tenant == null)
            {
                // Email enumeration hücumlarına qarşı: xəta atmırıq, sanki uğurlu kimi davam edirik
                _logger.LogWarning("Şifrə sıfırlama: Tenant tapılmadı. TenantSlug='{TenantSlug}'", tenantSlug);
                return;
            }

            var user = await _userRepository.GetByEmailAndTenantAsync(email, tenant.Id);
            if (user == null || !user.IsActive)
            {
                // Email enumeration hücumlarına qarşı: xəta atmırıq
                _logger.LogWarning("Şifrə sıfırlama: İstifadəçi tapılmadı və ya aktiv deyil. Email='{Email}', TenantId={TenantId}", email, tenant.Id);
                return;
            }

            var otp = new Random().Next(100000, 999999).ToString();

            var otpEntity = new PasswordResetOtp
            {
                UserId = user.Id,
                Code = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            await _otpRepository.AddAsync(otpEntity);
            await _unitOfWork.SaveChangesAsync();

            await _emailSender.SendOtpEmailAsync(email, otp);

            _logger.LogInformation("OTP uğurla göndərildi: UserId={UserId}", user.Id);
        }

        public async Task ResetPasswordAsync(string email, string tenantSlug, string otp, string newPassword)
        {
            _logger.LogInformation("Şifrə sıfırlama cəhdi: Email='{Email}', TenantSlug='{TenantSlug}'", email, tenantSlug);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(tenantSlug)
                || string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ValidationException("Bütün sahələr tələb olunur.");
            }

            var tenant = await _tenantRepository.GetBySlugAsync(tenantSlug);
            if (tenant == null)
            {
                throw new ValidationException("Kod yanlışdır və ya müddəti bitib.");
            }

            var user = await _userRepository.GetByEmailAndTenantAsync(email, tenant.Id);
            if (user == null || !user.IsActive)
            {
                throw new ValidationException("Kod yanlışdır və ya müddəti bitib.");
            }

            var otpEntity = await _otpRepository.GetValidOtpAsync(user.Id, otp);
            if (otpEntity == null)
            {
                _logger.LogWarning("OTP yanlışdır və ya müddəti bitib: UserId={UserId}", user.Id);
                throw new ValidationException("Kod yanlışdır və ya müddəti bitib.");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogError("Şifrə sıfırlama uğursuz oldu: UserId={UserId}, Xətalar={Errors}", user.Id, errors);
                throw new ValidationException($"Şifrə sıfırlanarkən xəta baş verdi: {errors}");
            }

            await _otpRepository.MarkAsUsedAsync(otpEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Şifrə uğurla sıfırlandı: UserId={UserId}", user.Id);
        }
    }
}
