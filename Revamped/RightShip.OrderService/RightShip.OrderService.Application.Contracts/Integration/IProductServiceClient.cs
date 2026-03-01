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
    /// Reserve (decrease) stock for a product.
    /// </summary>
    /// <exception cref="ProductNotFoundException">Product not found.</exception>
    /// <exception cref="InsufficientStockException">Insufficient stock.</exception>
    Task ReserveStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}
