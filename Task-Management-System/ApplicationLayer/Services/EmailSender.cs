using Contract.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Application.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var fromAddress = new MailAddress("giftcardmessenger@gmail.com");
            var toAddress = new MailAddress(toEmail);
            const string password = "gkvc ivgx eglv gnlf";

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromAddress.Address, password),
                EnableSsl = true
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Password Reset Code",
                IsBodyHtml = true,
                Body = $@"
            <h3>Password Reset</h3>
            <p>Sizin təsdiq kodunuz:</p>
            <h2 style='letter-spacing:3px'>{otp}</h2>
            <p>Kod 5 dəqiqə ərzində etibarlıdır.</p>
        "
            };

            await smtp.SendMailAsync(message);
        }

    }
}
