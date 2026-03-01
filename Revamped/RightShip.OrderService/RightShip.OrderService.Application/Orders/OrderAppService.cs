using RightShip.Core.Application.Uow;
using RightShip.OrderService.Application.Contracts.Integration;
using RightShip.OrderService.Application.Contracts.Orders;
using RightShip.OrderService.Domain.Entities;
using RightShip.OrderService.Domain.Exceptions;
using RightShip.OrderService.Domain.Repositories;

namespace RightShip.OrderService.Application.Orders;

/// <summary>
/// Application service for order management.
/// Coordinates domain objects, repositories, and Product Service integration.
/// </summary>
public class OrderAppService : IOrderAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductServiceClient _productServiceClient;

    public OrderAppService(IUnitOfWork unitOfWork, IProductServiceClient productServiceClient)
    {
        _unitOfWork = unitOfWork;
        _productServiceClient = productServiceClient;
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
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
        {
            throw new OrderLinesRequiredException();
        }

        // 1. Get prices and validate products exist
        var lineData = new List<(Guid productId, int quantity, decimal unitPrice)>();
        foreach (var line in lines)
        {
            var price = await _productServiceClient.GetProductPriceAsync(line.ProductId, cancellationToken);
            lineData.Add((line.ProductId, line.Quantity, price));
        }

        // 2. Reserve stock (validates sufficient quantity)
        foreach (var line in lines)
        {
            await _productServiceClient.ReserveStockAsync(line.ProductId, line.Quantity, cancellationToken);
        }

        // 3. Create and persist order
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
