namespace RightShip.OrderService.Domain.Exceptions;

/// <summary>
/// Thrown when an order line is created with a non-positive quantity.
/// </summary>
public class OrderLineQuantityMustBePositiveException : OrderDomainException
{
}

