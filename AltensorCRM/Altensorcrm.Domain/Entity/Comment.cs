using Altensorcrm.Domain.Common;

namespace Altensorcrm.Domain.Entity;

public class Comment : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public Guid? LeadId { get; set; }
    public Guid? DealId { get; set; }
    public Guid? TaskItemId { get; set; }
}
