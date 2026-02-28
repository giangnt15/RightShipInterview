using RightShip.Core.Domain.Events;

namespace RightShip.OrderService.Domain.Events
{
    /// <summary>
    /// Domain event raised when an order is created.
    /// </summary>
    public class OrderCreated : BaseEvent
    {
        public Guid CustomerId { get; set; }

        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Initial order lines created together with the order.
        /// </summary>
        public IReadOnlyCollection<OrderLinePayload> Lines { get; set; } = Array.Empty<OrderLinePayload>();

        /// <summary>
        /// Snapshot of the order total after creation.
        /// Everything that happened during the creation of the order is captured in this event.
        /// </summary>
        public decimal Total { get; set; }
    }
}

