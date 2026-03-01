namespace RightShip.OrderService.Application.Contracts.Orders;

/// <summary>
/// Application service for order management.
/// </summary>
public interface IOrderAppService
{
    /// <summary>
    /// Get order by id.
    /// </summary>
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new order.
    /// </summary>
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, Guid createdBy, CancellationToken cancellationToken = default);
}
