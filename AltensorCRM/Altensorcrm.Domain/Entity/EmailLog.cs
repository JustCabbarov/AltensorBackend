using System;
using Altensorcrm.Domain.Common;

namespace Altensorcrm.Domain.Entity
{
    public class EmailLog : ITenantEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string? CcEmail { get; set; }
        public string? BccEmail { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid? LeadId { get; set; }
        public Lead? Lead { get; set; }

        public Guid? DealId { get; set; }
        public Deal? Deal { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; }
    }
}
