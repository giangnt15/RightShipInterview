namespace RightShip.ProductService.Domain.Exceptions;

/// <summary>
/// Thrown when trying to confirm an expired reservation.
/// </summary>
public class ReservationExpiredException : ProductDomainException
{
    public Guid ReservationId { get; }

    public ReservationExpiredException(Guid reservationId)
        : base($"Reservation '{reservationId}' has expired.")
    {
        ReservationId = reservationId;
    }
}
