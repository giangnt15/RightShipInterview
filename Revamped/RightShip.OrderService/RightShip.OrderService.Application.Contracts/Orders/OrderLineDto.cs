namespace RightShip.OrderService.Application.Contracts.Orders;

/// <summary>
/// Data transfer object for an order line.
/// </summary>
public class OrderLineDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}
