namespace RightShip.OrderService.Domain.Exceptions;

/// <summary>
/// Thrown when an operation expects at least one order line but none are provided.
/// </summary>
public class OrderLinesRequiredException : OrderDomainException
{
}

