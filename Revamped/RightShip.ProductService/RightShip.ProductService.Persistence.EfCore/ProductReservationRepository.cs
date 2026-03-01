using Microsoft.EntityFrameworkCore;
using RightShip.ProductService.Domain.Entities;
using RightShip.ProductService.Domain.Enums;
using RightShip.ProductService.Domain.Repositories;

namespace RightShip.ProductService.Persistence.EfCore;

/// <summary>
/// EF Core implementation of product reservation repository.
/// </summary>
public class ProductReservationRepository : IProductReservationRepository
{
    private readonly ProductDbContext _context;

    public ProductReservationRepository(ProductDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ProductReservation> LoadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = await _context.ProductReservations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (r == null)
        {
            throw new InvalidOperationException($"Reservation '{id}' not found.");
        }
        return r;
    }

    /// <inheritdoc />
    public async Task<ProductReservation> AddAsync(ProductReservation entity)
    {
        await _context.ProductReservations.AddAsync(entity);
        return entity;
    }

    /// <inheritdoc />
    public Task<ProductReservation> UpdateAsync(ProductReservation entity, Guid id)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _context.ProductReservations.Update(entity);
        }
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public async Task<ProductReservation> DeleteAsync(Guid id)
    {
        var r = await LoadAsync(id);
        _context.ProductReservations.Remove(r);
        return r;
    }

    /// <inheritdoc />
    public async Task<int> GetTotalPendingQuantityForProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.ProductReservations
            .Where(r => r.ProductId == productId && r.Status == ReservationStatus.Pending && r.ExpiresAt > now)
            .SumAsync(r => r.Quantity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductReservation>> GetExpiredPendingReservationsAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.ProductReservations
            .Where(r => r.Status == ReservationStatus.Pending && r.ExpiresAt <= now)
            .OrderBy(r => r.ExpiresAt)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }
}
