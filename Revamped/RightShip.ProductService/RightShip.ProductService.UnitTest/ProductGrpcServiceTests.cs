using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Options;
using RightShip.Core.Application.Uow;
using RightShip.Core.Domain.ValueObjects;
using Moq;
using RightShip.ProductService.Application.Contracts.Grpc;
using RightShip.ProductService.Application.Options;
using RightShip.ProductService.Domain.Entities;
using RightShip.ProductService.Domain.Repositories;
using RightShip.ProductService.Domain.Services;
using RightShip.ProductService.Domain.Shared.ValueObjects;
using RightShip.ProductService.Application.Products;

namespace RightShip.ProductService.UnitTest;

public class ProductGrpcServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IProductRepository> _productRepositoryMock = null!;
    private Mock<IProductReservationRepository> _reservationRepositoryMock = null!;
    private ProductGrpcService _sut = null!;
    private ServerCallContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _reservationRepositoryMock = new Mock<IProductReservationRepository>();
        _unitOfWorkMock
            .Setup(x => x.StartAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _unitOfWorkMock
            .Setup(x => x.GetRepository<IProductRepository>())
            .Returns(_productRepositoryMock.Object);
        _unitOfWorkMock
            .Setup(x => x.GetRepository<IProductReservationRepository>())
            .Returns(_reservationRepositoryMock.Object);

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

        var reservationOptions = Options.Create(new ReservationOptions { DefaultTtlSeconds = 300 });
        var confirmationService = new ReservationConfirmationService();
        _sut = new ProductGrpcService(_unitOfWorkMock.Object, reservationOptions, confirmationService);
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
    public async Task CreateReservation_WhenValid_ReturnsReservationIdAndExpiresAt()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = Product.Create("Test", new Money { Amount = 10m }, new ProductQuantity(20), Guid.NewGuid());
        _productRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _reservationRepositoryMock
            .Setup(x => x.GetTotalPendingQuantityForProductAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        ProductReservation? captured = null;
        _reservationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ProductReservation>()))
            .Callback<ProductReservation>(r => captured = r)
            .ReturnsAsync((ProductReservation r) => r);

        var request = new CreateReservationRequest { ProductId = id.ToString(), Quantity = 5 };

        // Act
        var result = await _sut.CreateReservation(request, _context);

        // Assert
        Assert.That(result.ReservationId, Is.Not.Empty);
        Assert.That(result.ExpiresAt, Is.Not.Empty);
        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Quantity, Is.EqualTo(5));
        Assert.That(captured.ProductId, Is.EqualTo(id));
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CreateReservation_WhenQuantityZero_ThrowsRpcExceptionInvalidArgument()
    {
        var request = new CreateReservationRequest { ProductId = Guid.NewGuid().ToString(), Quantity = 0 };

        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.CreateReservation(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.InvalidArgument));
        Assert.That(ex.Status.Detail, Does.Contain("positive"));
    }

    [Test]
    public void CreateReservation_WhenQuantityNegative_ThrowsRpcExceptionInvalidArgument()
    {
        var request = new CreateReservationRequest { ProductId = Guid.NewGuid().ToString(), Quantity = -1 };

        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.CreateReservation(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.InvalidArgument));
    }

    [Test]
    public void CreateReservation_WhenInvalidProductId_ThrowsRpcExceptionInvalidArgument()
    {
        var request = new CreateReservationRequest { ProductId = "invalid", Quantity = 5 };

        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.CreateReservation(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.InvalidArgument));
    }

    [Test]
    public void CreateReservation_WhenInsufficientAvailableStock_ThrowsRpcExceptionFailedPrecondition()
    {
        var id = Guid.NewGuid();
        var product = Product.Create("Test", new Money { Amount = 10m }, new ProductQuantity(20), Guid.NewGuid());
        _productRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _reservationRepositoryMock
            .Setup(x => x.GetTotalPendingQuantityForProductAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(18); // 20 - 18 = 2 available

        var request = new CreateReservationRequest { ProductId = id.ToString(), Quantity = 5 };

        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.CreateReservation(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.FailedPrecondition));
        Assert.That(ex.Status.Detail, Does.Contain("Insufficient"));
    }

    [Test]
    public void CreateReservation_WhenProductNotFound_ThrowsRpcExceptionNotFound()
    {
        var id = Guid.NewGuid();
        _productRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        var request = new CreateReservationRequest { ProductId = id.ToString(), Quantity = 5 };

        var ex = Assert.ThrowsAsync<RpcException>(() => _sut.CreateReservation(request, _context));
        Assert.That(ex.Status.StatusCode, Is.EqualTo(StatusCode.NotFound));
    }

    [Test]
    public async Task ConfirmReservations_WhenValid_DeductsQuantity()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", new Money { Amount = 10m }, new ProductQuantity(20), Guid.NewGuid());
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));

        _reservationRepositoryMock
            .Setup(x => x.LoadAsync(reservation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _productRepositoryMock
            .Setup(x => x.LoadAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _reservationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ProductReservation>(), reservation.Id))
            .ReturnsAsync(reservation);
        _productRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), productId))
            .ReturnsAsync(product);

        var request = new ConfirmReservationsRequest { ReservationIds = { reservation.Id.ToString() } };

        await _sut.ConfirmReservations(request, _context);

        Assert.That(product.Quantity.Value, Is.EqualTo(15));
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void ConfirmReservations_WhenEmpty_ReturnsSuccessfully()
    {
        var request = new ConfirmReservationsRequest();

        Assert.DoesNotThrowAsync(() => _sut.ConfirmReservations(request, _context));
    }
}
