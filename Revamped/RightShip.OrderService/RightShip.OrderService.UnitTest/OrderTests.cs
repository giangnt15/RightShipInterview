using RightShip.Core.Domain.ValueObjects;
using RightShip.OrderService.Domain.Entities;
using RightShip.OrderService.Domain.Shared.Enums;
using RightShip.OrderService.Domain.Exceptions;

namespace RightShip.OrderService.UnitTest;

public class OrderTests
{
    [Test]
    public void Create_WithValidCustomerAndLines_ShouldInitializeOrderCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        var lines = new[]
        {
            CreateLine(Guid.NewGuid(), 2, 10m),
            CreateLine(Guid.NewGuid(), 1, 5m)
        };

        // Act
        var order = Order.Create(customerId, lines, createdBy);

        // Assert
        Assert.That(order.CustomerId, Is.EqualTo(customerId));
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Submitted));
        Assert.That(order.Lines.Count, Is.EqualTo(lines.Length));
        Assert.That(order.Total.Amount, Is.EqualTo(2 * 10m + 1 * 5m));
    }

    [Test]
    public void Create_WithEmptyCustomer_ShouldThrowCustomerIdRequiredException()
    {
        // Arrange
        var createdBy = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<CustomerIdRequiredException>(
            () => Order.Create(Guid.Empty, Enumerable.Empty<OrderLine>(), createdBy));
    }

    [Test]
    public void AddLines_WithValidLines_ShouldIncreaseLinesAndTotal()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var baseLine = CreateLine(Guid.NewGuid(), 1, 5m);
        var order = Order.Create(customerId, new[] { baseLine }, createdBy);

        var productId = Guid.NewGuid();
        var unitPrice = new Money { Amount = 12m };

        // Act
        order.AddLines(new[] { (productId, 3, unitPrice) });

        // Assert (1 base line + 1 newly added line)
        Assert.That(order.Lines.Count, Is.EqualTo(2));
        Assert.That(order.Total.Amount, Is.EqualTo(1 * 5m + 3 * 12m));
    }

    [Test]
    public void AddLines_WithNoLines_ShouldThrowOrderLinesRequiredException()
    {
        // Arrange
        var baseLine = CreateLine(Guid.NewGuid(), 1, 5m);
        var order = Order.Create(Guid.NewGuid(), new[] { baseLine }, Guid.NewGuid());

        // Act & Assert
        Assert.Throws<OrderLinesRequiredException>(() => order.AddLines(Array.Empty<(Guid, int, Money)>()));
    }

    [Test]
    public void RemoveLine_WithUnknownId_ShouldThrowOrderLineNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        var line = CreateLine(Guid.NewGuid(), 1, 10m);
        var order = Order.Create(customerId, new[] { line }, createdBy);

        var unknownLineId = Guid.NewGuid();

        // Act & Assert
        var ex = Assert.Throws<OrderLineNotFoundException>(() => order.RemoveLine(unknownLineId));
        Assert.That(ex!.OrderLineId, Is.EqualTo(unknownLineId));
    }

    private static OrderLine CreateLine(Guid productId, int quantity, decimal unitPrice)
    {
        var money = new Money { Amount = unitPrice };
        // Use reflection to call the internal constructor of OrderLine for test setup
        var ctor = typeof(OrderLine).GetConstructor(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            new[] { typeof(Guid), typeof(Guid), typeof(int), typeof(Money) },
            modifiers: null)!;

        return (OrderLine)ctor.Invoke(new object[] { Guid.NewGuid(), productId, quantity, money });
    }
}

