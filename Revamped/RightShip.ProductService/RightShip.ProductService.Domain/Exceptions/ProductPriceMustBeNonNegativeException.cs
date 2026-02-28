namespace RightShip.ProductService.Domain.Exceptions;

/// <summary>
/// Thrown when a product price would become negative.
/// </summary>
public class ProductPriceMustBeNonNegativeException : ProductDomainException
{
}

