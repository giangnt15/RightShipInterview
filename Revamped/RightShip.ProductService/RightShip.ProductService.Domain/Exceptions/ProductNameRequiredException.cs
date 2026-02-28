namespace RightShip.ProductService.Domain.Exceptions;

/// <summary>
/// Thrown when a product is created or modified without a name.
/// </summary>
public class ProductNameRequiredException : ProductDomainException
{
}

