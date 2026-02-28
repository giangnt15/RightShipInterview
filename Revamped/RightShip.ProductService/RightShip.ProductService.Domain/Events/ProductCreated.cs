using RightShip.Core.Domain.Events;

namespace RightShip.ProductService.Domain.Events;

/// <summary>
/// Domain event raised when a product is created.
/// </summary>
public class ProductCreated : BaseEvent
{
    public Guid ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Price amount of the product at creation time.
    /// Stored as a primitive amount to avoid leaking value objects.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Initial stock quantity.
    /// </summary>
    public int Quantity { get; set; }
}

