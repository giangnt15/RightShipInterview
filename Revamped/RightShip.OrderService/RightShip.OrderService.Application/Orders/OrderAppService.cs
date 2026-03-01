using RightShip.Core.Application.Uow;
using RightShip.OrderService.Application.Contracts.Orders;
using RightShip.OrderService.Domain.Entities;
using RightShip.OrderService.Domain.Repositories;

namespace RightShip.OrderService.Application.Orders;

/// <summary>
/// Application service for order management.
/// Coordinates domain objects and repositories via unit of work.
/// </summary>
public class OrderAppService : IOrderAppService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderAppService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {

        await _unitOfWork.StartAsync(cancellationToken);
        var repo = _unitOfWork.GetRepository<IOrderRepository>();
        var order = await repo.LoadAsync(id, cancellationToken);
        return MapToDto(order);
    }

    /// <inheritdoc />
    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var lineData = (dto.Lines ?? [])
            .Select(l => (l.ProductId, l.Quantity, l.UnitPrice));

        var order = Order.Create(dto.CustomerId, lineData, createdBy);

        await _unitOfWork.StartAsync(cancellationToken);
        var repo = _unitOfWork.GetRepository<IOrderRepository>();
        var added = await repo.AddAsync(order);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToDto(added);
    }

    private static OrderDto? MapToDto(Order? order)
    {
        if (order == null)
        {
            return null;
        }
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            Total = order.Total.Amount,
            Lines = order.Lines
                .Select(l => new OrderLineDto
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice.Amount,
                    SubTotal = l.SubTotal.Amount
                })
                .ToList()
        };
    }
}
