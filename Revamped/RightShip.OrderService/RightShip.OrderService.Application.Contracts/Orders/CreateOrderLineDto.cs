namespace RightShip.OrderService.Application.Contracts.Orders;

/// <summary>
/// Input for a single order line when creating an order.
/// UnitPrice is fetched from Product Service.
/// </summary>
public class CreateOrderLineDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
