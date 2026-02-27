namespace RightShip.Core.Domain.Entities
{
    /// <summary>
    /// Interface for versioned entity.
    /// Used to implement optimistic locking and tracking changes to entities over time.
    /// </summary>
    public interface IVersioned
    {
        public long Version { get; set; }
    }
}
