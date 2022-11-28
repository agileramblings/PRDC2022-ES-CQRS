namespace DepthConsulting.Core.Messaging;

public abstract class Message
{
    /// <summary>
    ///     Unique id for this Message.
    ///     All messages have unique Ids
    /// </summary>
    public Guid MessageId { get; set; } = Guid.NewGuid();

    /// <summary>
    ///     Correlation id is used to group a collection of messages
    ///     This is not unique in messages and can be defined by
    ///     the producer of the initial message
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    ///     This id is the copied from the Message
    ///     That is the cause for a new Message to be created
    ///     This will create a chain of causation
    ///     in a group of messages
    ///     This may be Guid.Empty if this is the initial
    ///     message in the chain of causation
    /// </summary>
    public Guid CausationId { get; set; } = Guid.Empty;
}