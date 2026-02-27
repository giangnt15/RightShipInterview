namespace RightShip.Core.Domain.Entities
{
    /// <summary>
    /// The base class for entities when using ORM, it implements the IEntity interface.
    /// </summary>
    public class BaseEntity : IEntity
    {
    }

    /// <summary>
    /// The base class for entities with generic data type of field Id when using ORM, it implements the IEntity<T> interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseEntity<T> : BaseEntity, IEntity<T>
    {
        public required T Id { get; set; }
    }
}
