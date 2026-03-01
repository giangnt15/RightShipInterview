namespace RightShip.OrderService.Application.Contracts.Integration;

/// <summary>
/// Thrown when insufficient stock to fulfill reservation.
/// </summary>
public class InsufficientStockException : Exception
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }

    public InsufficientStockException(Guid productId, int requestedQuantity)
        : base($"Insufficient stock for product '{productId}'. Requested: {requestedQuantity}.")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
    }
}
