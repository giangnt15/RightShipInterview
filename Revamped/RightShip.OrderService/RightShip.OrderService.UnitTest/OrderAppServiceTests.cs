using RightShip.Core.Application.Uow;
using Moq;
using RightShip.OrderService.Application.Contracts.Orders;
using RightShip.OrderService.Application.Orders;
using RightShip.OrderService.Domain.Entities;
using RightShip.OrderService.Domain.Repositories;
using RightShip.OrderService.Domain.Shared.Enums;

namespace RightShip.OrderService.UnitTest;

public class OrderAppServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IOrderRepository> _orderRepositoryMock = null!;
    private OrderAppService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock
            .Setup(x => x.StartAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _unitOfWorkMock
            .Setup(x => x.GetRepository<IOrderRepository>())
            .Returns(_orderRepositoryMock.Object);

        _sut = new OrderAppService(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GetByIdAsync_WhenOrderExists_ReturnsOrderDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var order = Order.Create(
            Guid.NewGuid(),
            [(Guid.NewGuid(), 2, 10m), (Guid.NewGuid(), 1, 5m)],
            Guid.NewGuid());
        _orderRepositoryMock
            .Setup(x => x.LoadAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(order.Id));
        Assert.That(result.CustomerId, Is.EqualTo(order.CustomerId));
        Assert.That(result.Status, Is.EqualTo(order.Status));
        Assert.That(result.Total, Is.EqualTo(2 * 10m + 1 * 5m));
        Assert.That(result.Lines, Has.Count.EqualTo(2));
        _unitOfWorkMock.Verify(x => x.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_WhenOrderNotFound_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _orderRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateOrderAsync_CreatesAndReturnsOrderDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var dto = new CreateOrderDto
        {
            CustomerId = customerId,
            Lines =
            [
                new CreateOrderLineDto { ProductId = productId, Quantity = 2, UnitPrice = 10m },
                new CreateOrderLineDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 5m }
            ]
        };
        Order? capturedOrder = null;
        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(o => capturedOrder = o)
            .ReturnsAsync((Order o) => o);

        // Act
        var result = await _sut.CreateOrderAsync(dto, createdBy);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CustomerId, Is.EqualTo(customerId));
        Assert.That(result.Status, Is.EqualTo(OrderStatus.Submitted));
        Assert.That(result.Total, Is.EqualTo(2 * 10m + 1 * 5m));
        Assert.That(result.Lines, Has.Count.EqualTo(2));
        Assert.That(capturedOrder, Is.Not.Null);
        Assert.That(capturedOrder!.CustomerId, Is.EqualTo(customerId));
        Assert.That(capturedOrder.Lines.Count, Is.EqualTo(2));
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
