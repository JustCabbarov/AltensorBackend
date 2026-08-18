using Contract.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPasswordResetRepository _otpRepo;
        private readonly IEmailSender _emailSender;

        public PasswordResetService(
            UserManager<ApplicationUser> userManager,
            IPasswordResetRepository otpRepo,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _otpRepo = otpRepo;
            _emailSender = emailSender;
        }

        public async Task SendOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("User tapılmadı");

            var otp = new Random().Next(100000, 999999).ToString();
            var expiration = DateTime.UtcNow.AddMinutes(5);

            var otpEntity = new PasswordResetOTP
            {
                UserId = user.Id,
                Code = otp,
                Expiration = expiration
            };

            await _otpRepo.AddAsync(otpEntity);

            await _emailSender.SendOtpEmailAsync(email,otp );
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("User tapılmadı");

            var otpEntity = await _otpRepo.GetValidOtpAsync(user.Id, otp);
            if (otpEntity == null)
                throw new InvalidOperationException("Kod yanlışdır və ya müddəti bitib");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (result.Succeeded)
            {
                await _otpRepo.RemoveAsync(otpEntity);
            }

            return result;
        }
    }

}
