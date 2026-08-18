using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using Altensorcrm.Contract.DTOs.Email;
using Altensorcrm.Contract.Services.Email;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Altensorcrm.Application.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        public Task<EmailLogDetailDto> SendEmailAsync(string toEmail, string subject, string body, Guid? leadId = null, Guid? dealId = null, Guid? userId = null)
        {
            return SendEmailAsync(new SendEmailDto
            {
                ToEmail = toEmail,
                Subject = subject,
                Body = body,
                LeadId = leadId,
                DealId = dealId
            }, userId);
        }

        public async Task<EmailLogDetailDto> SendEmailAsync(SendEmailDto dto, Guid? userId = null)
        {
            if (string.IsNullOrWhiteSpace(dto.ToEmail))
            {
                throw new ArgumentException("Recipient email address (ToEmail) is required.");
            }

            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 587;
            var smtpUser = _configuration["Email:SmtpUser"] ?? "";
            var smtpPass = _configuration["Email:SmtpPass"] ?? "";
            var fromEmail = _configuration["Email:From"] ?? smtpUser;
            var fromName = _configuration["Email:FromName"] ?? "Altensor CRM Platform";
            var enableSsl = !bool.TryParse(_configuration["Email:EnableSsl"], out var ssl) || ssl;

            // Attempt to send real email via SMTP
            try
            {
                using var mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, fromName);
                mail.To.Add(dto.ToEmail.Trim());

                if (!string.IsNullOrWhiteSpace(dto.CcEmail))
                {
                    foreach (var cc in dto.CcEmail.Split(',', ';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        mail.CC.Add(cc.Trim());
                    }
                }

                if (!string.IsNullOrWhiteSpace(dto.BccEmail))
                {
                    foreach (var bcc in dto.BccEmail.Split(',', ';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        mail.Bcc.Add(bcc.Trim());
                    }
                }

                mail.Subject = dto.Subject ?? "";
                mail.Body = dto.Body ?? "";
                mail.IsBodyHtml = dto.Body != null && (dto.Body.Contains("<p>") || dto.Body.Contains("<div>") || dto.Body.Contains("<br>"));

                using var smtp = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    Timeout = 20000
                };

                await smtp.SendMailAsync(mail);
                _logger.LogInformation("Email successfully dispatched via SMTP to {ToEmail}", dto.ToEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP Email delivery notice for {ToEmail}.", dto.ToEmail);
            }

            // Always record the sent email log in PostgreSQL database
            var emailLog = new EmailLog
            {
                Id = Guid.NewGuid(),
                ToEmail = dto.ToEmail.Trim(),
                CcEmail = dto.CcEmail?.Trim(),
                BccEmail = dto.BccEmail?.Trim(),
                FromEmail = fromEmail,
                Subject = dto.Subject?.Trim() ?? "",
                Body = dto.Body ?? "",
                SentAt = DateTime.UtcNow,
                LeadId = dto.LeadId,
                DealId = dto.DealId,
                UserId = userId
            };

            await _unitOfWork.EmailLogs.AddAsync(emailLog);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<EmailLogDetailDto>(emailLog);
        }

        public async Task<List<EmailLogDetailDto>> GetLogsByLeadIdAsync(Guid leadId)
        {
            var logs = await _unitOfWork.EmailLogs.GetByLeadIdAsync(leadId);
            return _mapper.Map<List<EmailLogDetailDto>>(logs);
        }

        public async Task<List<EmailLogDetailDto>> GetLogsByDealIdAsync(Guid dealId)
        {
            var logs = await _unitOfWork.EmailLogs.GetByDealIdAsync(dealId);
            return _mapper.Map<List<EmailLogDetailDto>>(logs);
        }
    }
}
