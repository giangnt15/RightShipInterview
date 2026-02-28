using RightShip.Core.Domain.Events;

namespace RightShip.ProductService.Domain.Events;

/// <summary>
/// Domain event raised when a product price is changed.
/// </summary>
public class ProductPriceChanged : BaseEvent
{
    /// <summary>
    /// New price amount of the product.
    /// </summary>
    public decimal NewPrice { get; set; }
}

