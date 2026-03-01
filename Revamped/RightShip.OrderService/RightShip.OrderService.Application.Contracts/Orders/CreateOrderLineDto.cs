namespace RightShip.OrderService.Application.Contracts.Orders;

/// <summary>
/// Input for a single order line when creating an order.
/// </summary>
public class CreateOrderLineDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
