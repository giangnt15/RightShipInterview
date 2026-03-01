using RightShip.Core.Domain.Events;

namespace RightShip.ProductService.Domain.Events;

/// <summary>
/// Domain event raised when a reservation expires without confirmation.
/// </summary>
public class ProductReservationExpired : BaseEvent
{
}
