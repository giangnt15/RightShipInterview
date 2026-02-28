namespace RightShip.OrderService.Domain.Exceptions;

/// <summary>
/// Thrown when the total amount of an order would become negative.
/// </summary>
public class NegativeOrderTotalException : OrderDomainException
{
}

