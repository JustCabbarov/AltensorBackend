using Altensorcrm.Domain.Common;

namespace Altensorcrm.Domain.Entity;

public class TaskChecklist : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; } = false;

    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
}
