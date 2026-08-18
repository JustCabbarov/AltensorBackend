using AutoMapper;
using Altensorcrm.Application.Exceptions;
using Altensorcrm.Contract.DTOs.Product;
using Altensorcrm.Contract.Services.Product;
using Altensorcrm.Domain.Repository;
using ProductEntity = Altensorcrm.Domain.Entity.Product;

namespace Altensorcrm.Application.Services.Product;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductDetailDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Repository<ProductEntity>().GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ProductDetailDto>>(products.OrderByDescending(p => p.CreatedAt).ToList());
    }

    public async Task<ProductDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Repository<ProductEntity>().GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException(nameof(ProductEntity), id);
        }

        return _mapper.Map<ProductDetailDto>(product);
    }

    public async Task<ProductDetailDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = _mapper.Map<ProductEntity>(dto);
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(product.ProductName))
        {
            product.ProductName = product.ProductCode;
        }

        await _unitOfWork.Repository<ProductEntity>().AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDetailDto>(product);
    }

    public async Task<ProductDetailDto> UpdateAsync(UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Repository<ProductEntity>().GetByIdAsync(dto.Id, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException(nameof(ProductEntity), dto.Id);
        }

        _mapper.Map(dto, product);
        _unitOfWork.Repository<ProductEntity>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDetailDto>(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Repository<ProductEntity>().GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        _unitOfWork.Repository<ProductEntity>().Delete(product);
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
