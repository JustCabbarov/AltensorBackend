using Altensorcrm.Domain.Common;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Domain.Entity;

public class CallLog : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    public CallType Type { get; set; } = CallType.Incoming;
    public string ToNumber { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public CallStatus Status { get; set; } = CallStatus.Completed;

    public int DurationInSeconds { get; set; }

    public Guid? CallReceivedById { get; set; }
    public User? CallReceivedBy { get; set; }

    public Guid? CallerUserId { get; set; }
    public User? CallerUser { get; set; }

    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
