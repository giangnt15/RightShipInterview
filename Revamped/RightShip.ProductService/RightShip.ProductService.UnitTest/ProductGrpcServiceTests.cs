using Grpc.Core;
using Grpc.Core.Testing;
using RightShip.Core.Application.Uow;
using RightShip.Core.Domain.ValueObjects;
using Moq;
using RightShip.ProductService.Application.Contracts.Grpc;
using RightShip.ProductService.Domain.Entities;
using RightShip.ProductService.Domain.Exceptions;
using RightShip.ProductService.Domain.Repositories;
using RightShip.ProductService.Domain.Shared.ValueObjects;
using RightShip.ProductService.Application.Products;

namespace RightShip.ProductService.UnitTest;

public class ProductGrpcServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IProductRepository> _productRepositoryMock = null!;
    private ProductGrpcService _sut = null!;
    private ServerCallContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock
            .Setup(x => x.StartAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _unitOfWorkMock
            .Setup(x => x.GetRepository<IProductRepository>())
            .Returns(_productRepositoryMock.Object);

        _context = TestServerCallContext.Create(
            "Test",
            "localhost",
            DateTime.UtcNow.AddMinutes(30),
            new Metadata(),
            CancellationToken.None,
            "10.0.0.1:5000",
            null,
            null,
            _ => Task.CompletedTask,
            () => new WriteOptions(),
            _ => { });

        _sut = new ProductGrpcService(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GetProductPrice_WhenProductExists_ReturnsResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = Product.Create("Test", new Money { Amount = 25.5m }, new ProductQuantity(5), Guid.NewGuid());
        _productRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var request = new GetProductPriceRequest { ProductId = id.ToString() };

        // Act
        var result = await _sut.GetProductPrice(request, _context);

        // Assert
        Assert.That(result.ProductId, Is.EqualTo(product.Id.ToString()));
        Assert.That(result.Price, Is.EqualTo(25.5));
    }

    [Test]
    public void GetProductPrice_WhenProductNotFound_ThrowsRpcExceptionNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        var request = new GetProductPriceRequest { ProductId = id.ToString() };

        // Act & Assert
        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.GetProductPrice(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.NotFound));
    }

    [Test]
    public void GetProductPrice_WhenInvalidProductId_ThrowsRpcExceptionInvalidArgument()
    {
        // Arrange
        var request = new GetProductPriceRequest { ProductId = "invalid-guid" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.GetProductPrice(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.InvalidArgument));
        Assert.That(ex.Status.Detail, Does.Contain("Invalid product id"));
    }

    [Test]
    public async Task ReserveStock_WhenValid_CommitsAndReturns()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = Product.Create("Test", new Money { Amount = 10m }, new ProductQuantity(20), Guid.NewGuid());
        _productRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), id))
            .ReturnsAsync(product);

        var request = new ReserveStockRequest { ProductId = id.ToString(), Quantity = 5 };

        // Act
        var result = await _sut.ReserveStock(request, _context);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(product.Quantity.Value, Is.EqualTo(15));
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void ReserveStock_WhenQuantityZero_ThrowsRpcExceptionInvalidArgument()
    {
        // Arrange
        var request = new ReserveStockRequest { ProductId = Guid.NewGuid().ToString(), Quantity = 0 };

        // Act & Assert
        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.ReserveStock(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.InvalidArgument));
        Assert.That(ex.Status.Detail, Does.Contain("positive"));
    }

    [Test]
    public void ReserveStock_WhenQuantityNegative_ThrowsRpcExceptionInvalidArgument()
    {
        // Arrange
        var request = new ReserveStockRequest { ProductId = Guid.NewGuid().ToString(), Quantity = -1 };

        // Act & Assert
        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.ReserveStock(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.InvalidArgument));
    }

    [Test]
    public void ReserveStock_WhenInvalidProductId_ThrowsRpcExceptionInvalidArgument()
    {
        // Arrange
        var request = new ReserveStockRequest { ProductId = "invalid", Quantity = 5 };

        // Act & Assert
        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.ReserveStock(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.InvalidArgument));
    }

    [Test]
    public void ReserveStock_WhenInsufficientStock_ThrowsRpcExceptionFailedPrecondition()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = Product.Create("Test", new Money { Amount = 10m }, new ProductQuantity(2), Guid.NewGuid());
        _productRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var request = new ReserveStockRequest { ProductId = id.ToString(), Quantity = 5 };

        // Act & Assert
        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.ReserveStock(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.FailedPrecondition));
        Assert.That(ex.Status.Detail, Does.Contain("Insufficient stock"));
    }

    [Test]
    public void ReserveStock_WhenProductNotFound_ThrowsRpcExceptionNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        var request = new ReserveStockRequest { ProductId = id.ToString(), Quantity = 5 };

        // Act & Assert
        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.ReserveStock(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.NotFound));
    }
}
