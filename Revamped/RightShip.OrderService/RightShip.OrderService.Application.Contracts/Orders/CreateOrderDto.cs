namespace RightShip.OrderService.Application.Contracts.Orders;

/// <summary>
/// Input for creating an order.
/// </summary>
public class CreateOrderDto
{
    public Guid CustomerId { get; set; }
    public IReadOnlyList<CreateOrderLineDto> Lines { get; set; } = [];
}
