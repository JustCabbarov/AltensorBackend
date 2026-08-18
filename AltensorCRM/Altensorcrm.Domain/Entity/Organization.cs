using Altensorcrm.Domain.Common;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Domain.Entity;

public class Organization : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    public string OrganizationName { get; set; } = string.Empty;
    public decimal AnnualRevenue { get; set; } = 0;
    public string? Website { get; set; }

    public Guid? TerritoryId { get; set; }
    public Territory? Territory { get; set; }

    public EmployeeCountRange? NoOfEmployees { get; set; }
    public IndustryType? Industry { get; set; }

    public Guid? AddressId { get; set; }
    public Address? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
}
