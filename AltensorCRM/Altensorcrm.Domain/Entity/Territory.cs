using Altensorcrm.Domain.Common;

namespace Altensorcrm.Domain.Entity;

public class Territory : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string TerritoryName { get; set; } = string.Empty;

    public Guid? TerritoryManagerId { get; set; }
    public User? TerritoryManager { get; set; }

    public Guid? ParentTerritoryId { get; set; }
    public Territory? ParentTerritory { get; set; }
    public ICollection<Territory> ChildTerritories { get; set; } = new List<Territory>();

    public bool IsGroup { get; set; } = false;

    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
