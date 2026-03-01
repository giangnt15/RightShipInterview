using RightShip.Core.Application.Uow;
using RightShip.Core.Domain.ValueObjects;
using RightShip.ProductService.Application.Contracts.Products;
using RightShip.ProductService.Domain.Entities;
using RightShip.ProductService.Domain.Repositories;
using RightShip.ProductService.Domain.Shared.ValueObjects;

namespace RightShip.ProductService.Application.Products;

/// <summary>
/// Application service for product queries and stock management.
/// Currently there is no CQRS. This is a simple application service that is used to query and manage products.
/// In a real system, we might need a dedicated view DB for querying products.
/// </summary>
public class ProductAppService : IProductAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.StartAsync(cancellationToken);
            var repo = _unitOfWork.GetRepository<IProductRepository>();
            var product = await repo.LoadAsync(id, cancellationToken);
            return MapToDto(product);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<ProductDto>> GetListAsync(ProductListFilterDto filter, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.StartAsync(cancellationToken);
        var repo = _unitOfWork.GetRepository<IProductRepository>();

        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var (items, totalCount) = await repo.GetListAsync(
            filter.SearchName,
            pageNumber,
            pageSize,
            filter.SortBy ?? "Name",
            filter.SortDirection ?? "Asc",
            cancellationToken);

        return new PagedResultDto<ProductDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.StartAsync(cancellationToken);
        var repo = _unitOfWork.GetRepository<IProductRepository>();
        var price = new Money { Amount = dto.Price };
        var quantity = new ProductQuantity(dto.InitialQuantity);
        var product = Product.Create(dto.Name, price, quantity, createdBy);
        var added = await repo.AddAsync(product);
        await _unitOfWork.CommitAsync(cancellationToken);
        return MapToDto(added);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price.Amount,
            Quantity = product.Quantity.Value
        };
    }
}
