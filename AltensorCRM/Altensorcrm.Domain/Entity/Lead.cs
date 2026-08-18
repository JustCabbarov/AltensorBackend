using Altensorcrm.Domain.Common;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Domain.Entity;

public class Lead : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    public Salutation? Salutation { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public Gender? Gender { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string? Website { get; set; }
    public EmployeeCountRange? NoOfEmployees { get; set; }

    public Guid? TerritoryId { get; set; }
    public Territory? Territory { get; set; }

    public decimal AnnualRevenue { get; set; } = 0;
    public IndustryType? Industry { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;

    public Guid? LeadOwnerId { get; set; }
    public User? LeadOwner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<CallLog> CallLogs { get; set; } = new List<CallLog>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
