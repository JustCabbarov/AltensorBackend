using System;
using System.Collections.Generic;
using System.Text;

namespace Contract.Services
{
    public interface IEmailSender
    {
        public Task SendOtpEmailAsync(string toEmail, string otp);
    }
}
