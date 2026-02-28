namespace RightShip.ProductService.Domain.Exceptions;

/// <summary>
/// Thrown when a product quantity would become negative.
/// </summary>
public class ProductQuantityMustBeNonNegativeException : ProductDomainException
{
}

