namespace RightShip.ProductService.Domain.Enums;

/// <summary>
/// Status of a product reservation.
/// </summary>
public enum ReservationStatus
{
    /// <summary>Reserved but not yet claimed by an order.</summary>
    Pending = 0,

    /// <summary>Reservation claimed; product quantity was deducted.</summary>
    Confirmed = 1,

    /// <summary>Expired without confirmation; quantity was never deducted.</summary>
    Expired = 2,
}
