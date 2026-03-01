namespace RightShip.Core.Domain.Entities;

/// <summary>
/// The outbox message entity, used to store the messages to be sent to the message broker.
/// This is saved to the database in the same transaction as the aggregate roots to ensure at least once delivery of messages.
/// </summary>
public class OutboxMessage : BaseEntity<long>
{
    /// <summary>
    /// The topic of the message
    /// </summary>
    public required string Topic { get; set; }
    /// <summary>
    /// The correlation id of the message
    /// </summary>
    public required string CorrelationId { get; set; }
    /// <summary>
    /// The payload of the message
    /// </summary>
    public required string Payload { get; set; }
    /// <summary>
    /// Whether the message has been sent
    /// </summary>
    public bool Sent { get; set; }
    /// <summary>
    /// Whether the message is being processed
    /// </summary>
    public bool Processing { get; set; }
    /// <summary>
    /// The creation date of the message
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// The update date of the message
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}