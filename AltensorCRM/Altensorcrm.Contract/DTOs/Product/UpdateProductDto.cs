namespace Altensorcrm.Contract.DTOs.Product;

public class UpdateProductDto
{
    public Guid Id { get; set; }
    public string NamingSeries { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal StandardSellingRate { get; set; }
    public bool Disabled { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
