using RightShip.Core.Domain.Events;

namespace RightShip.Core.Domain.Entities
{
    public interface IAggregateRoot : IEntity, IVersioned
    {
        IEnumerable<IEvent> GetDistributedEvents();
        void ClearDistributedEvents();
        string GetEventTopic();
    }

    public interface IAggregateRoot<TId> : IEntity<TId>, IAggregateRoot
    {

    }
}
