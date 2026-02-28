using RightShip.ProductService.Domain.Shared.ValueObjects;

namespace RightShip.ProductService.UnitTest;

public class ProductQuantityTests
{
    [Test]
    public void Constructor_WithNonNegativeValue_ShouldSetValue()
    {
        var quantity = new ProductQuantity(5);

        Assert.That(quantity.Value, Is.EqualTo(5));
    }

    [Test]
    public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ProductQuantity(-1));
    }

    [Test]
    public void Zero_ShouldReturnQuantityWithZeroValue()
    {
        var zero = ProductQuantity.Zero();

        Assert.That(zero.Value, Is.EqualTo(0));
    }

    [Test]
    public void AddOperator_ShouldSumValues()
    {
        var a = new ProductQuantity(3);
        var b = new ProductQuantity(4);

        var result = a + b;

        Assert.That(result.Value, Is.EqualTo(7));
    }

    [Test]
    public void SubtractOperator_WithResultNonNegative_ShouldSubtractValues()
    {
        var a = new ProductQuantity(5);
        var b = new ProductQuantity(2);

        var result = a - b;

        Assert.That(result.Value, Is.EqualTo(3));
    }

    [Test]
    public void SubtractOperator_WithResultNegative_ShouldThrowArgumentException()
    {
        var a = new ProductQuantity(2);
        var b = new ProductQuantity(3);

        Assert.Throws<ArgumentException>(() => _ = a - b);
    }
}

