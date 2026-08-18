using Altensorcrm.Domain.Common;

namespace Altensorcrm.Domain.Entity;

public class DealProduct : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    public Guid DealId { get; set; }
    public Deal? Deal { get; set; }

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public decimal Rate { get; set; } = 0;
    public int Quantity { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
