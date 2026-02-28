namespace RightShip.Core.Domain.Events
{
    /// <summary>
    /// Base class for domain events, providing common properties and functionality.
    /// </summary>
    public abstract class BaseEvent : IEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Id of the object/aggregate generating this event
        /// </summary>
        public required object SourceId { get; set; }
        /// <summary>
        /// Id of the message when proccessed will yeild this event
        /// </summary>
        public Guid? TriggeringMessageId { get; set; }
        /// <summary>
        /// Version of the AggregateRoot at the time this event is generated
        /// </summary>
        public long Version { get; set; }
        /// <summary>
        /// Timestamp of the event
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        public Guid? PerformedBy { get; set; }
    }
}
