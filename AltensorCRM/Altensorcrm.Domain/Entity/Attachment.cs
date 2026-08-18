using Altensorcrm.Domain.Common;

namespace Altensorcrm.Domain.Entity;

public class Attachment : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Guid? LeadId { get; set; }
    public Guid? DealId { get; set; }
    public Guid? TaskItemId { get; set; }
}
