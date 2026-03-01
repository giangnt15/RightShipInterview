using RightShip.ProductService.Domain.Entities;

namespace RightShip.ProductService.Domain.Services;

/// <summary>
/// Coordinates confirmation of ProductReservation and Product quantity deduction.
/// </summary>
public class ReservationConfirmationService : IReservationConfirmationService
{
    /// <inheritdoc />
    public void ConfirmReservation(ProductReservation reservation, Product product)
    {
        reservation.Confirm();
        product.AdjustQuantity(-reservation.Quantity);
    }
}
