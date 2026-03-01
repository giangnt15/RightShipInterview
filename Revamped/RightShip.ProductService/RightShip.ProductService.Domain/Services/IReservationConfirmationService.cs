using RightShip.ProductService.Domain.Entities;

namespace RightShip.ProductService.Domain.Services;

/// <summary>
/// Domain service that coordinates reservation confirmation and product quantity deduction.
/// Keeps ProductReservation aggregate independent of Product.
/// </summary>
public interface IReservationConfirmationService
{
    /// <summary>
    /// Confirms a reservation (transitions to Confirmed) and deducts quantity from product.
    /// </summary>
    /// <param name="reservation">Reservation to confirm.</param>
    /// <param name="product">Product to deduct quantity from (must match reservation.ProductId).</param>
    void ConfirmReservation(ProductReservation reservation, Product product);
}
