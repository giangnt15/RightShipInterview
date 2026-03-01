using RightShip.Core.Domain.Repositories;
using RightShip.ProductService.Domain.Entities;

namespace RightShip.ProductService.Domain.Repositories;

/// <summary>
/// Repository for ProductReservation aggregate.
/// </summary>
public interface IProductReservationRepository : IRepository<ProductReservation, Guid>
{
    /// <summary>
    /// Sum of Quantity for all Pending reservations of a product (not expired).
    /// Used to compute available quantity = Product.Quantity - this.
    /// </summary>
    Task<int> GetTotalPendingQuantityForProductAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pending reservations that have passed ExpiresAt.
    /// Used by background job to release expired reservations.
    /// </summary>
    Task<IReadOnlyList<ProductReservation>> GetExpiredPendingReservationsAsync(int maxCount, CancellationToken cancellationToken = default);
}
