using RightShip.ProductService.Domain.Entities;
using RightShip.ProductService.Domain.Enums;
using RightShip.ProductService.Domain.Exceptions;
using System.Reflection;

namespace RightShip.ProductService.UnitTest;

public class ProductReservationTests
{
    [Test]
    public void Create_WithValidData_InitializesReservationCorrectly()
    {
        var productId = Guid.NewGuid();
        var quantity = 5;
        var ttl = TimeSpan.FromMinutes(5);
        var before = DateTime.UtcNow;

        var reservation = ProductReservation.Create(productId, quantity, ttl);

        var after = DateTime.UtcNow;
        Assert.That(reservation.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(reservation.ProductId, Is.EqualTo(productId));
        Assert.That(reservation.Quantity, Is.EqualTo(quantity));
        Assert.That(reservation.Status, Is.EqualTo(ReservationStatus.Pending));
        Assert.That(reservation.ExpiresAt, Is.GreaterThanOrEqualTo(before.Add(ttl)));
        Assert.That(reservation.ExpiresAt, Is.LessThanOrEqualTo(after.Add(ttl).AddMilliseconds(100)));
        Assert.That(reservation.CreatedAt, Is.GreaterThanOrEqualTo(before));
        Assert.That(reservation.CreatedAt, Is.LessThanOrEqualTo(after.AddMilliseconds(100)));
    }

    [Test]
    public void Create_WithZeroQuantity_ThrowsProductQuantityMustBeNonNegativeException()
    {
        var productId = Guid.NewGuid();

        var ex = Assert.Throws<ProductQuantityMustBeNonNegativeException>(
            () => ProductReservation.Create(productId, 0, TimeSpan.FromMinutes(5)));

        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Create_WithNegativeQuantity_ThrowsProductQuantityMustBeNonNegativeException()
    {
        var productId = Guid.NewGuid();

        Assert.Throws<ProductQuantityMustBeNonNegativeException>(
            () => ProductReservation.Create(productId, -1, TimeSpan.FromMinutes(5)));
    }

    [Test]
    public void Confirm_WhenPendingAndNotExpired_TransitionsToConfirmed()
    {
        var productId = Guid.NewGuid();
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));

        reservation.Confirm();

        Assert.That(reservation.Status, Is.EqualTo(ReservationStatus.Confirmed));
        Assert.That(reservation.ProductId, Is.EqualTo(productId));
        Assert.That(reservation.Quantity, Is.EqualTo(5));
    }

    [Test]
    public void Confirm_WhenExpired_ThrowsReservationExpiredException()
    {
        var productId = Guid.NewGuid();
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));
        SetExpiresAt(reservation, DateTime.UtcNow.AddMinutes(-1));

        var ex = Assert.Throws<ReservationExpiredException>(() => reservation.Confirm());

        Assert.That(ex.ReservationId, Is.EqualTo(reservation.Id));
        Assert.That(reservation.Status, Is.EqualTo(ReservationStatus.Pending));
    }

    [Test]
    public void Confirm_WhenAlreadyConfirmed_ThrowsReservationAlreadyConfirmedException()
    {
        var productId = Guid.NewGuid();
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));
        reservation.Confirm();

        var ex = Assert.Throws<ReservationAlreadyConfirmedException>(() => reservation.Confirm());

        Assert.That(ex.ReservationId, Is.EqualTo(reservation.Id));
        Assert.That(reservation.Status, Is.EqualTo(ReservationStatus.Confirmed));
    }

    [Test]
    public void Confirm_WhenAlreadyExpired_ThrowsReservationExpiredException()
    {
        var productId = Guid.NewGuid();
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));
        reservation.MarkExpired();

        var ex = Assert.Throws<ReservationExpiredException>(() => reservation.Confirm());

        Assert.That(ex.ReservationId, Is.EqualTo(reservation.Id));
        Assert.That(reservation.Status, Is.EqualTo(ReservationStatus.Expired));
    }

    [Test]
    public void MarkExpired_WhenPending_TransitionsToExpired()
    {
        var productId = Guid.NewGuid();
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));

        reservation.MarkExpired();

        Assert.That(reservation.Status, Is.EqualTo(ReservationStatus.Expired));
    }

    [Test]
    public void MarkExpired_WhenAlreadyConfirmed_DoesNothing()
    {
        var productId = Guid.NewGuid();
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));
        reservation.Confirm();

        reservation.MarkExpired();

        Assert.That(reservation.Status, Is.EqualTo(ReservationStatus.Confirmed));
    }

    [Test]
    public void MarkExpired_WhenAlreadyExpired_DoesNothing()
    {
        var productId = Guid.NewGuid();
        var reservation = ProductReservation.Create(productId, 5, TimeSpan.FromMinutes(5));
        reservation.MarkExpired();

        reservation.MarkExpired();

        Assert.That(reservation.Status, Is.EqualTo(ReservationStatus.Expired));
    }

    [Test]
    public void Create_GeneratesUniqueIdsForEachReservation()
    {
        var productId = Guid.NewGuid();
        var r1 = ProductReservation.Create(productId, 1, TimeSpan.FromMinutes(1));
        var r2 = ProductReservation.Create(productId, 1, TimeSpan.FromMinutes(1));

        Assert.That(r1.Id, Is.Not.EqualTo(r2.Id));
    }

    private static void SetExpiresAt(ProductReservation reservation, DateTime expiresAt)
    {
        var prop = typeof(ProductReservation).GetProperty("ExpiresAt")!;
        var setter = prop.GetSetMethod(nonPublic: true);
        setter!.Invoke(reservation, [expiresAt]);
    }
}
