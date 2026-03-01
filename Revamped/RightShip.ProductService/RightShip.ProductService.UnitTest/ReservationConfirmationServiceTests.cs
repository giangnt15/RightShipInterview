using RightShip.Core.Domain.ValueObjects;
using RightShip.ProductService.Domain.Entities;
using RightShip.ProductService.Domain.Enums;
using RightShip.ProductService.Domain.Exceptions;
using RightShip.ProductService.Domain.Services;
using RightShip.ProductService.Domain.Shared.ValueObjects;
using System.Reflection;

namespace RightShip.ProductService.UnitTest;

public class ReservationConfirmationServiceTests
{
    private ReservationConfirmationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ReservationConfirmationService();
    }

    [Test]
    public void ConfirmReservation_WhenValid_TransitionsReservationAndDeductsProductQuantity()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", new Money { Amount = 10m }, new ProductQuantity(20), Guid.NewGuid());
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));

        _sut.ConfirmReservation(reservation, product);

        Assert.That(reservation.Status, Is.EqualTo(ReservationStatus.Confirmed));
        Assert.That(product.Quantity.Value, Is.EqualTo(15));
    }

    [Test]
    public void ConfirmReservation_WhenReservationExpired_ThrowsReservationExpiredException()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", new Money { Amount = 10m }, new ProductQuantity(20), Guid.NewGuid());
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));
        SetReservationExpiresAt(reservation, DateTime.UtcNow.AddMinutes(-1));

        var ex = Assert.Throws<ReservationExpiredException>(() => _sut.ConfirmReservation(reservation, product));

        Assert.That(ex.ReservationId, Is.EqualTo(reservation.Id));
        Assert.That(product.Quantity.Value, Is.EqualTo(20)); // unchanged
    }

    [Test]
    public void ConfirmReservation_WhenReservationAlreadyConfirmed_ThrowsReservationAlreadyConfirmedException()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", new Money { Amount = 10m }, new ProductQuantity(20), Guid.NewGuid());
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));
        _sut.ConfirmReservation(reservation, product);

        var ex = Assert.Throws<ReservationAlreadyConfirmedException>(() => _sut.ConfirmReservation(reservation, product));

        Assert.That(ex.ReservationId, Is.EqualTo(reservation.Id));
        Assert.That(product.Quantity.Value, Is.EqualTo(15)); // only deducted once
    }

    [Test]
    public void ConfirmReservation_WhenInsufficientProductQuantity_ThrowsProductQuantityMustBeNonNegativeException()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create("P", new Money { Amount = 10m }, new ProductQuantity(2), Guid.NewGuid());
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));

        Assert.Throws<ProductQuantityMustBeNonNegativeException>(() => _sut.ConfirmReservation(reservation, product));

        // reservation.Confirm() runs first; product.AdjustQuantity throws after - product quantity unchanged
        Assert.That(product.Quantity.Value, Is.EqualTo(2));
    }

    private static void SetReservationExpiresAt(ProductReservation reservation, DateTime expiresAt)
    {
        var prop = typeof(ProductReservation).GetProperty("ExpiresAt")!;
        var setter = prop.GetSetMethod(nonPublic: true);
        setter!.Invoke(reservation, [expiresAt]);
    }
}
