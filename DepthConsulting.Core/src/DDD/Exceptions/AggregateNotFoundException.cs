namespace DepthConsulting.Core.DDD.Exceptions;
#pragma warning disable CA1032
public class AggregateNotFoundException : Exception
{
    public readonly string AggregateId;

    public AggregateNotFoundException(string aggregateId, Exception? innerException = null) : base(
        $"There were no events discovered for the requested aggregate ({aggregateId})", innerException)
    {
        if (string.IsNullOrEmpty(aggregateId)) throw new ArgumentException("You cannot create a AggregateNotFoundException without an Aggregate Id", nameof(aggregateId));

        AggregateId = aggregateId;
    }
}