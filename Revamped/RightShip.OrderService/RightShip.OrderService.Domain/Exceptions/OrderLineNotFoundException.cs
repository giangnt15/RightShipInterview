namespace RightShip.OrderService.Domain.Exceptions;

/// <summary>
/// Thrown when an expected order line cannot be found in the aggregate.
/// </summary>
public class OrderLineNotFoundException : OrderDomainException
{
    public OrderLineNotFoundException(Guid orderLineId)
    {
        OrderLineId = orderLineId;
    }

    public Guid OrderLineId { get; }
}

