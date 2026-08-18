using System;

namespace Altensorcrm.Contract.DTOs.Email
{
    public class SendEmailDto
    {
        public string ToEmail { get; set; } = string.Empty;
        public string? CcEmail { get; set; }
        public string? BccEmail { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Guid? LeadId { get; set; }
        public Guid? DealId { get; set; }
    }

    public class EmailLogDetailDto
    {
        public Guid Id { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string? CcEmail { get; set; }
        public string? BccEmail { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public Guid? LeadId { get; set; }
        public Guid? DealId { get; set; }
        public Guid? UserId { get; set; }
    }
}
