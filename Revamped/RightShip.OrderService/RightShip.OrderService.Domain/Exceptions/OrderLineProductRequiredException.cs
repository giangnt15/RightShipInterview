namespace RightShip.OrderService.Domain.Exceptions;

/// <summary>
/// Thrown when an order line is created without a valid product id.
/// </summary>
public class OrderLineProductRequiredException : OrderDomainException
{
}

