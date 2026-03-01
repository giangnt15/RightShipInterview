using RightShip.Core.Domain.Repositories;
using RightShip.ProductService.Domain.Entities;

namespace RightShip.ProductService.Domain.Repositories;

/// <summary>
/// Repository interface for Product aggregate roots.
/// </summary>
public interface IProductRepository : IRepository<Product, Guid>
{
    /// <summary>
    /// Get paged list of products with filters.
    /// </summary>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetListAsync(
        string? searchName,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default);
}

