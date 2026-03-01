using RightShip.Core.Domain.Exceptions;

namespace RightShip.ProductService.Domain.Exceptions;

/// <summary>
/// Base type for all ProductService domain exceptions.
/// </summary>
public class ProductDomainException : DomainException
{
    public ProductDomainException()
    {
    }

    public ProductDomainException(string? message)
        : base(message)
    {
    }
}

