namespace RightShip.OrderService.Application.Contracts.Integration;

/// <summary>
/// Thrown when a product is not found in the Product Service.
/// </summary>
public class ProductNotFoundException : Exception
{
    public Guid ProductId { get; }

    public ProductNotFoundException(Guid productId)
        : base($"Product '{productId}' not found.")
    {
        ProductId = productId;
    }
}
