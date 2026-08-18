using Altensorcrm.Domain.Common;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Domain.Entity;

public class TaskItem : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; } = false;

    public Guid AssignedUserId { get; set; }
    public User AssignedUser { get; set; } = null!;

    public string? DepartmentName { get; set; }

    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public Guid? DealId { get; set; }
    public Deal? Deal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskChecklist> Checklists { get; set; } = new List<TaskChecklist>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
