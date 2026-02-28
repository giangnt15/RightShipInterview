using RightShip.Core.Domain.Events;

namespace RightShip.ProductService.Domain.Events;

/// <summary>
/// Domain event raised when product stock quantity is adjusted.
/// </summary>
public class ProductQuantityAdjusted : BaseEvent
{
    /// <summary>
    /// Delta applied to the quantity (can be positive or negative).
    /// </summary>
    public int Delta { get; set; }
}

