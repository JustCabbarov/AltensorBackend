using Altensorcrm.Domain.Common;

namespace Altensorcrm.Domain.Entity;

public class Product : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string NamingSeries { get; set; } = "CRM-PROD-.YYYY.-";
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal StandardSellingRate { get; set; } = 0;
    public bool Disabled { get; set; } = false;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
