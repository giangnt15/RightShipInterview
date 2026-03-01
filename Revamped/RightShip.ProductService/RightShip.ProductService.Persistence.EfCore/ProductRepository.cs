using Microsoft.EntityFrameworkCore;
using RightShip.ProductService.Domain.Entities;
using RightShip.ProductService.Domain.Repositories;

namespace RightShip.ProductService.Persistence.EfCore;

/// <summary>
/// EF Core implementation of product repository.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Product> LoadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with id '{id}' was not found.");
        }
        return product;
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product entity)
    {
        await _context.Products.AddAsync(entity);
        return entity;
    }

    /// <inheritdoc />
    public Task<Product> UpdateAsync(Product entity, Guid id)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _context.Products.Update(entity);
        }
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public async Task<Product> DeleteAsync(Guid id)
    {
        var product = await LoadAsync(id);
        _context.Products.Remove(product);
        return product;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetListAsync(
        string? searchName,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var term = searchName.Trim();
            query = query.Where(p => p.Name.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var ascending = !string.Equals(sortDirection, "Desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy?.ToLowerInvariant() switch
        {
            "price" => ascending ? query.OrderBy(p => p.Price.Amount) : query.OrderByDescending(p => p.Price.Amount),
            "quantity" => ascending ? query.OrderBy(p => p.Quantity.Value) : query.OrderByDescending(p => p.Quantity.Value),
            "createdat" => ascending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
            _ => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
        };

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
