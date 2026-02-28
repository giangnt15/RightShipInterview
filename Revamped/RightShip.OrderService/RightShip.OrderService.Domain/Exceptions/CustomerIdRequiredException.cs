namespace RightShip.OrderService.Domain.Exceptions;

/// <summary>
/// Thrown when an order is created or modified without a customer id.
/// </summary>
public class CustomerIdRequiredException : OrderDomainException
{
}

