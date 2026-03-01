using RightShip.Core.Domain.Events;

namespace RightShip.ProductService.Domain.Events;

/// <summary>
/// Domain event raised when a reservation is confirmed (order committed; quantity deducted).
/// </summary>
public class ProductReservationConfirmed : BaseEvent
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
