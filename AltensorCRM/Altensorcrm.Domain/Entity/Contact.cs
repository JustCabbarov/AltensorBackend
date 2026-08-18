using Altensorcrm.Domain.Common;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Domain.Entity;

public class Contact : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    public Salutation? Salutation { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;

    public string MobileNo { get; set; } = string.Empty;
    public Gender? Gender { get; set; }

    public string? CompanyName { get; set; }
    public string? Designation { get; set; }

    public Guid? AddressId { get; set; }
    public Address? Address { get; set; }

    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public Guid? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
}
