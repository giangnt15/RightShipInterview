using RightShip.Core.Domain.Events;

namespace RightShip.OrderService.Domain.Events;

/// <summary>
/// Domain event raised when one or more order lines are added to an order.
/// </summary>
public class OrderLineAdded : BaseEvent
{
    public IReadOnlyCollection<OrderLinePayload> Lines { get; set; } = Array.Empty<OrderLinePayload>();

    /// <summary>
    /// Snapshot of the order total after the lines have been added.
    /// Stored as a primitive amount to avoid leaking value objects.
    /// </summary>
    public decimal Total { get; set; }
}

