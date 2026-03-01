namespace RightShip.Core.Domain.Exceptions
{
    /// <summary>
    /// The base class for all exceptions in the domain layer, it inherits from the System.Exception class.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException()
        {
        }

        public DomainException(string message)
            : base(message)
        {
        }
    }
}
