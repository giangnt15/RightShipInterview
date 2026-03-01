using System.Diagnostics.CodeAnalysis;
using RightShip.Core.Domain.Entities;
using RightShip.Core.Domain.Events;
using RightShip.ProductService.Domain.Enums;
using RightShip.ProductService.Domain.Events;
using RightShip.ProductService.Domain.Exceptions;

namespace RightShip.ProductService.Domain.Entities;

/// <summary>
/// Product reservation aggregate. Holds quantity tentatively reserved for an order.
/// If the order is not confirmed before ExpiresAt, the reservation expires and quantity is released.
/// Trade-off: Two-phase (CreateReservation → ConfirmReservation) avoids losing stock when Order Service fails to commit.
/// </summary>
public class ProductReservation : AggregateRoot<Guid>
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    [SetsRequiredMembers]
    private ProductReservation()
    {
    }

    /// <summary>
    /// Create a reservation. Does not deduct Product.Quantity; available = Product.Quantity - sum(Pending reservations).
    /// </summary>
    public static ProductReservation Create(Guid productId, int quantity, TimeSpan ttl)
    {
        if (quantity <= 0)
        {
            throw new ProductQuantityMustBeNonNegativeException();
        }

        var id = Guid.NewGuid();
        var reservation = new ProductReservation();
        var expiresAt = DateTime.UtcNow.Add(ttl);

        var @event = new ProductReservationCreated
        {
            SourceId = id,
            ProductId = productId,
            Quantity = quantity,
            ExpiresAt = expiresAt
        };

        reservation.Apply(@event);
        return reservation;
    }

    /// <summary>
    /// Transition reservation to Confirmed. Validation only; use IReservationConfirmationService to also deduct Product quantity.
    /// </summary>
    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
        {
            if (Status == ReservationStatus.Expired)
            {
                throw new ReservationExpiredException(Id);
            }
            throw new ReservationAlreadyConfirmedException(Id);
        }

        if (DateTime.UtcNow > ExpiresAt)
        {
            throw new ReservationExpiredException(Id);
        }

        var @event = new ProductReservationConfirmed
        {
            SourceId = Id,
            ProductId = ProductId,
            Quantity = Quantity
        };

        Apply(@event);
    }

    /// <summary>
    /// Mark as expired. No Product change; quantity was never deducted.
    /// </summary>
    public void MarkExpired()
    {
        if (Status != ReservationStatus.Pending)
        {
            return;
        }

        var @event = new ProductReservationExpired { SourceId = Id };
        Apply(@event);
    }

    protected override void When(IEvent @event)
    {
        switch (@event)
        {
            case ProductReservationCreated created:
                Id = (Guid)created.SourceId;
                ProductId = created.ProductId;
                Quantity = created.Quantity;
                ExpiresAt = created.ExpiresAt;
                CreatedAt = created.Timestamp.UtcDateTime;
                Status = ReservationStatus.Pending;
                break;

            case ProductReservationConfirmed:
                Status = ReservationStatus.Confirmed;
                break;

            case ProductReservationExpired:
                Status = ReservationStatus.Expired;
                break;
        }
    }

    protected override void EnsureValidState()
    {
        if (Quantity <= 0)
        {
            throw new ProductQuantityMustBeNonNegativeException();
        }
    }
}
