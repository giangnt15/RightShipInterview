namespace RightShip.OrderService.Domain.Exceptions;

/// <summary>
/// Thrown when an order is in an invalid state without any order lines.
/// </summary>
public class OrderMustHaveAtLeastOneLineException : OrderDomainException
{
}

