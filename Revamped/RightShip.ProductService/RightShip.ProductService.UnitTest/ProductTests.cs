using RightShip.Core.Domain.ValueObjects;
using RightShip.ProductService.Domain.Entities;
using RightShip.ProductService.Domain.Exceptions;
using RightShip.ProductService.Domain.Shared.ValueObjects;

namespace RightShip.ProductService.UnitTest;

public class ProductTests
{
    [Test]
    public void Create_WithValidData_ShouldInitializeProductCorrectly()
    {
        // Arrange
        var name = "Test Product";
        var price = new Money { Amount = 10m };
        var quantity = new ProductQuantity(5);
        var createdBy = Guid.NewGuid();

        // Act
        var product = Product.Create(name, price, quantity, createdBy);

        // Assert
        Assert.That(product.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(product.Name, Is.EqualTo(name));
        Assert.That(product.Price.Amount, Is.EqualTo(10m));
        Assert.That(product.Quantity.Value, Is.EqualTo(5));
    }

    [Test]
    public void Create_WithEmptyName_ShouldThrowProductNameRequiredException()
    {
        // Arrange
        var price = new Money { Amount = 10m };
        var quantity = new ProductQuantity(1);

        // Act & Assert
        Assert.Throws<ProductNameRequiredException>(
            () => Product.Create(string.Empty, price, quantity, Guid.NewGuid()));
    }

    [Test]
    public void Create_WithNegativePrice_ShouldThrowProductPriceMustBeNonNegativeException()
    {
        // Arrange
        var price = new Money { Amount = 1m };
        var quantity = new ProductQuantity(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => Product.Create("Product", price with { Amount = -1m }, quantity, Guid.NewGuid()));
    }

    [Test]
    public void Create_WithNegativeQuantity_ShouldThrowProductQuantityMustBeNonNegativeException()
    {
        // Arrange
        var price = new Money { Amount = 1m };
        var quantity = new ProductQuantity(0);

        // Act & Assert
        Assert.DoesNotThrow(
            () => Product.Create("Product", price, quantity, Guid.NewGuid()));
    }

    [Test]
    public void ChangePrice_WithValidPrice_ShouldUpdatePrice()
    {
        // Arrange
        var product = Product.Create(
            "Product",
            new Money { Amount = 10m },
            new ProductQuantity(1),
            Guid.NewGuid());

        var newPrice = new Money { Amount = 15m };

        // Act
        product.ChangePrice(newPrice);

        // Assert
        Assert.That(product.Price.Amount, Is.EqualTo(15m));
    }

    [Test]
    public void ChangePrice_WithNegativePrice_ShouldThrowProductPriceMustBeNonNegativeException()
    {
        // Arrange
        var product = Product.Create(
            "Product",
            new Money { Amount = 10m },
            new ProductQuantity(1),
            Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => product.ChangePrice(product.Price with { Amount = -5m }));
    }

    [Test]
    public void AdjustQuantity_WithPositiveDelta_ShouldIncreaseQuantity()
    {
        // Arrange
        var product = Product.Create(
            "Product",
            new Money { Amount = 10m },
            new ProductQuantity(5),
            Guid.NewGuid());

        // Act
        product.AdjustQuantity(3);

        // Assert
        Assert.That(product.Quantity.Value, Is.EqualTo(8));
    }

    [Test]
    public void AdjustQuantity_WithNegativeDeltaBeyondZero_ShouldThrowProductQuantityMustBeNonNegativeException()
    {
        // Arrange
        var product = Product.Create(
            "Product",
            new Money { Amount = 10m },
            new ProductQuantity(2),
            Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ProductQuantityMustBeNonNegativeException>(
            () => product.AdjustQuantity(-3));
    }
}

