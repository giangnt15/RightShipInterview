using RightShip.Core.Domain.Exceptions;

namespace RightShip.OrderService.Domain.Exceptions
{
    /// <summary>
    /// Base type for all OrderService domain exceptions.
    /// </summary>
    public class OrderDomainException : DomainException
    {
        public OrderDomainException()
        {
        }

        public OrderDomainException(string message)
        {
        }

        public OrderDomainException(string message, Exception innerException)
        {
        }
    }
}

