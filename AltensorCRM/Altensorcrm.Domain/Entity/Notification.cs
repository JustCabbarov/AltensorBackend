using Altensorcrm.Domain.Common;

namespace Altensorcrm.Domain.Entity;

public class Notification : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime NotifyAt { get; set; }
    public bool IsRead { get; set; } = false;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public Guid? TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
}
