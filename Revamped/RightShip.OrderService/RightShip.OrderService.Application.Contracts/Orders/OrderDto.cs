using RightShip.OrderService.Domain.Shared.Enums;

namespace RightShip.OrderService.Application.Contracts.Orders;

/// <summary>
/// Data transfer object for order read operations.
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyList<OrderLineDto> Lines { get; set; } = [];
}
