namespace RightShip.OrderService.Domain.Events;

/// <summary>
/// Snapshot of an order line used inside domain events.
/// </summary>
public class OrderLinePayload
{
    public Guid OrderLineId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    /// <summary>
    /// Unit price amount for this line. We avoid
    /// referencing domain value objects in events
    /// and only store primitive representations.
    /// </summary>
    public decimal UnitPrice { get; set; }
}

