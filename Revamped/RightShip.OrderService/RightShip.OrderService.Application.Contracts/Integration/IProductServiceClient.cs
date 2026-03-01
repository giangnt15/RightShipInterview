namespace RightShip.OrderService.Application.Contracts.Integration;

/// <summary>
/// Client for Product Service (gRPC). Used by Order Service to validate products and reserve stock.
/// </summary>
public interface IProductServiceClient
{
    /// <summary>
    /// Get product price by id.
    /// </summary>
    /// <exception cref="ProductNotFoundException">Product not found.</exception>
    Task<decimal> GetProductPriceAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a time-limited reservation. Does not deduct quantity until <see cref="ConfirmReservationsAsync"/>.
    /// </summary>
    /// <param name="productId">Product id.</param>
    /// <param name="quantity">Quantity to reserve.</param>
    /// <param name="ttlSeconds">TTL in seconds; default 300 if null.</param>
    /// <returns>Reservation id to pass to ConfirmReservationsAsync.</returns>
    /// <exception cref="ProductNotFoundException">Product not found.</exception>
    /// <exception cref="InsufficientStockException">Insufficient available stock.</exception>
    Task<Guid> CreateReservationAsync(Guid productId, int quantity, int? ttlSeconds = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm reservations; deducts product quantity. Call after Order Service commits.
    /// </summary>
    /// <param name="reservationIds">Reservation ids from CreateReservationAsync.</param>
    /// <exception cref="ProductNotFoundException">Reservation or product not found.</exception>
    Task ConfirmReservationsAsync(IReadOnlyList<Guid> reservationIds, CancellationToken cancellationToken = default);
}
