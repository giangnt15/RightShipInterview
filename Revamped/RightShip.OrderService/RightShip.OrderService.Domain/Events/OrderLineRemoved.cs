using RightShip.Core.Domain.Events;

namespace RightShip.OrderService.Domain.Events;

/// <summary>
/// Domain event raised when an order line is removed from an order.
/// </summary>
public class OrderLineRemoved : BaseEvent
{
    public Guid OrderLineId { get; set; }

    /// <summary>
    /// Snapshot of the order total after the line has been removed.
    /// Stored as a primitive amount to avoid leaking value objects.
    /// </summary>
    public decimal Total { get; set; }
}

