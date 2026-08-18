using Altensorcrm.Domain.Common;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Domain.Entity;

public class Deal : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    public bool ChooseExistingOrganization { get; set; }
    public bool ChooseExistingContact { get; set; }

    public string OrganizationName { get; set; } = string.Empty;
    public string? Website { get; set; }
    public EmployeeCountRange? NoOfEmployees { get; set; }

    public Guid? TerritoryId { get; set; }
    public Territory? Territory { get; set; }

    public decimal AnnualRevenue { get; set; } = 0;
    public IndustryType? Industry { get; set; }

    public Salutation? Salutation { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PrimaryEmail { get; set; } = string.Empty;
    public string PrimaryMobileNo { get; set; } = string.Empty;
    public Gender? Gender { get; set; }

    public DealStatus Status { get; set; } = DealStatus.Qualification;
    public string? LostReason { get; set; }

    public Guid? DealOwnerId { get; set; }
    public User? DealOwner { get; set; }

    public Guid? SourceLeadId { get; set; }
    public Lead? SourceLead { get; set; }

    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<CallLog> CallLogs { get; set; } = new List<CallLog>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<DealProduct> Products { get; set; } = new List<DealProduct>();
}
