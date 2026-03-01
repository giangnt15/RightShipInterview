namespace RightShip.ProductService.Domain.Exceptions;

/// <summary>
/// Thrown when trying to confirm an already confirmed reservation.
/// </summary>
public class ReservationAlreadyConfirmedException : ProductDomainException
{
    public Guid ReservationId { get; }

    public ReservationAlreadyConfirmedException(Guid reservationId)
        : base($"Reservation '{reservationId}' is already confirmed.")
    {
        ReservationId = reservationId;
    }
}
