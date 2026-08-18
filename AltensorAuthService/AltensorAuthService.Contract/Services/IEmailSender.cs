namespace AltensorAuthService.Contract.Services
{
    public interface IEmailSender
    {
        Task SendOtpEmailAsync(string toEmail, string otp);
    }
}
