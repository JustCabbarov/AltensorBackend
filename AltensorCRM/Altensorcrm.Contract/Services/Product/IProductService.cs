using Altensorcrm.Contract.DTOs.Product;

namespace Altensorcrm.Contract.Services.Product;

public interface IProductService
{
    Task<IReadOnlyList<ProductDetailDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDetailDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDetailDto> UpdateAsync(UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
