namespace RightShip.Core.Domain.ValueObjects
{
    /// <summary>
    /// The base class for value objects.
    /// Use record to implement value objects, as it provides built-in support for value equality and immutability.
    /// Properties in value objects should be immutable, setters should be init-only.
    /// For performance critical value objects, consider using a struct instead of a class to avoid heap allocations.
    /// </summary>
    public record class BaseValueObject
    {
    }
}
