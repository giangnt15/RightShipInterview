using RightShip.Core.Domain.Events;

namespace RightShip.ProductService.Domain.Events;

/// <summary>
/// Domain event raised when a product reservation is created.
/// </summary>
public class ProductReservationCreated : BaseEvent
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
}
