using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Altensorcrm.Contract.DTOs.Email;

namespace Altensorcrm.Contract.Services.Email
{
    public interface IEmailService
    {
        Task<EmailLogDetailDto> SendEmailAsync(SendEmailDto dto, Guid? userId = null);
        Task<EmailLogDetailDto> SendEmailAsync(string toEmail, string subject, string body, Guid? leadId = null, Guid? dealId = null, Guid? userId = null);
        Task<List<EmailLogDetailDto>> GetLogsByLeadIdAsync(Guid leadId);
        Task<List<EmailLogDetailDto>> GetLogsByDealIdAsync(Guid dealId);
    }
}
