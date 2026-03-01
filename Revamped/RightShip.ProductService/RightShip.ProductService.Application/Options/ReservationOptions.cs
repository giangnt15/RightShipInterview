namespace RightShip.ProductService.Application.Options;

/// <summary>
/// Reservation-related configuration. Binds to "Reservation" section in appsettings.
/// </summary>
public class ReservationOptions
{
    public const string SectionName = "Reservation";

    /// <summary>
    /// Default TTL for new reservations in seconds. Used when client omits ttl_seconds.
    /// </summary>
    public int DefaultTtlSeconds { get; set; } = 300;
}
