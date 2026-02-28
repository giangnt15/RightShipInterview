using RightShip.Core.Domain.Events;

namespace RightShip.Core.Domain.Entities
{
    /// <summary>
    /// The base class for aggregate roots, it implements the IAggregateRoot interface.
    /// We use aggregate roots to represent the main entities in our domain, 
    /// they are responsible for maintaining the consistency of the aggregate and handling the domain events.
    /// Acts as a consistency boundary for the aggregate,
    /// ensuring that all invariants are maintained and that the aggregate is always in a valid state.
    /// </summary>
    public abstract class AggregateRoot : BaseEntity, IAggregateRoot
    {
        public long Version { get; set; }
        private readonly List<IEvent> _distributedEvents = [];
        public void ClearDistributedEvents() => _distributedEvents?.Clear();
        public IEnumerable<IEvent> GetDistributedEvents() => _distributedEvents?.ToList() ?? [];

        /// <summary>
        /// Method to actually apply event to this aggregate
        /// Must be pure, contain no side effects since it will 
        /// be called when rehydrating the aggregate from the event store if we use event sourcing later,
        /// and also when applying new events.
        /// </summary>
        /// <param name="event">The event to be applied</param>
        protected abstract void When(IEvent @event);

        private void HandleApply(IEvent @event)
        {
            When(@event);
            Version = @event.Version;
            if (this is IHasModificationInfo entity)
            {
                entity.UpdatedAt = @event.Timestamp.UtcDateTime;
            }
        }

        protected abstract void EnsureValidState();

        /// <summary>
        /// Apply event to this aggregate
        /// </summary>
        /// <param name="event">The event to be applied</param>
        protected virtual void Apply(IEvent @event)
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));
            ArgumentNullException.ThrowIfNull(@event.SourceId, nameof(@event.SourceId));
            @event.Version = Version + 1;
            HandleApply(@event);
            EnsureValidState();
            AddDistributedEvent(@event);
        }

        protected virtual void AddDistributedEvent(IEvent @event)
        {
            _distributedEvents.Add(@event);
        }

        public string GetEventTopic()
        {
            return $"{GetType().Name}_events";
        }

    }

    public abstract class AggregateRoot<TId> : AggregateRoot, IAggregateRoot<TId>
    {
        public required TId Id { get; set; }

        protected override void Apply(IEvent @event)
        {
            // In case of create, should not reassign event SourceId
            if (Id != null && !Id.Equals(default(TId)))
            {
                @event.SourceId = Id;
            }
            else if (typeof(TId) == typeof(Guid) && (@event.SourceId == null || @event.SourceId.Equals(default(TId))))
            {
                @event.SourceId = Guid.NewGuid();
            }
            base.Apply(@event);
        }
    }
}
