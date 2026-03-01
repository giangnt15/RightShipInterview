using RightShip.Core.Application.Uow;
using RightShip.Core.Domain.ValueObjects;
using Moq;
using RightShip.ProductService.Application.Contracts.Products;
using RightShip.ProductService.Application.Products;
using RightShip.ProductService.Domain.Entities;
using RightShip.ProductService.Domain.Repositories;
using RightShip.ProductService.Domain.Shared.ValueObjects;

namespace RightShip.ProductService.UnitTest;

public class ProductAppServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IProductRepository> _productRepositoryMock = null!;
    private ProductAppService _sut = null!;

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

        _sut = new ProductAppService(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProductDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = Product.Create("Test", new Money { Amount = 10m }, new ProductQuantity(5), Guid.NewGuid());
        _productRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(product.Id));
        Assert.That(result.Name, Is.EqualTo("Test"));
        Assert.That(result.Price, Is.EqualTo(10m));
        Assert.That(result.Quantity, Is.EqualTo(5));
        _unitOfWorkMock.Verify(x => x.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_WhenProductNotFound_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productRepositoryMock
            .Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetListAsync_ReturnsPagedResult()
    {
        // Arrange
        var product = Product.Create("Product A", new Money { Amount = 10m }, new ProductQuantity(2), Guid.NewGuid());
        _productRepositoryMock
            .Setup(x => x.GetListAsync(
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product> { product }, 1));

        var filter = new ProductListFilterDto { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetListAsync(filter);

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.TotalCount, Is.EqualTo(1));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
        Assert.That(result.Items[0].Name, Is.EqualTo("Product A"));
    }

    [Test]
    public async Task GetListAsync_WhenPageNumberLessThanOne_ClampsToOne()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetListAsync(
                It.IsAny<string?>(),
                1,
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 0));

        var filter = new ProductListFilterDto { PageNumber = 0, PageSize = 10 };

        // Act
        var result = await _sut.GetListAsync(filter);

        // Assert
        Assert.That(result.PageNumber, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateProductAsync_CreatesAndReturnsProductDto()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var dto = new CreateProductDto { Name = "New Product", Price = 99m, InitialQuantity = 10 };
        Product? capturedProduct = null;
        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => capturedProduct = p)
            .ReturnsAsync((Product p) => p);

        // Act
        var result = await _sut.CreateProductAsync(dto, createdBy);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("New Product"));
        Assert.That(result.Price, Is.EqualTo(99m));
        Assert.That(result.Quantity, Is.EqualTo(10));
        Assert.That(capturedProduct, Is.Not.Null);
        Assert.That(capturedProduct!.Name, Is.EqualTo("New Product"));
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
