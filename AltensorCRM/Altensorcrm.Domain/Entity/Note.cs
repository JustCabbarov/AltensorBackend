using Altensorcrm.Domain.Common;

namespace Altensorcrm.Domain.Entity;

public class Note : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
