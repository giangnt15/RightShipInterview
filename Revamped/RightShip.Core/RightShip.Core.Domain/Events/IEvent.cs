namespace RightShip.Core.Domain.Events
{
    /// <summary>
    /// Interface for domain events, representing significant occurrences or changes in the state of an aggregate or entity.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Unique identifier for the event, used for idempotency and tracking
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Id of the object/aggregate generating this event
        /// </summary>
        public object SourceId { get; set; }
        /// <summary>
        /// Id of the message when proccessed will yeild this event
        /// </summary>
        public Guid? TriggeringMessageId { get; set; }
        /// <summary>
        /// Timestamp of the event
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
        /// <summary>
        /// Version of the AggregateRoot at the time this event is generated
        /// </summary>
        public long Version { get; set; }

        public Guid? PerformedBy { get; set; }
    }
}
