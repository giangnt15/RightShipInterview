namespace RightShip.ProductService.Application.Contracts.Products;

/// <summary>
/// Application service for product queries and stock management.
/// </summary>
public interface IProductAppService
{
    /// <summary>
    /// Get product by id.
    /// </summary>
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paged list of products with optional filters.
    /// </summary>
    Task<PagedResultDto<ProductDto>> GetListAsync(ProductListFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new product.
    /// </summary>
    Task<ProductDto> CreateProductAsync(CreateProductDto dto, Guid createdBy, CancellationToken cancellationToken = default);
}
