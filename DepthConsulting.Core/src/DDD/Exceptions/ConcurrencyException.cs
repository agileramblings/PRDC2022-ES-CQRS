namespace DepthConsulting.Core.DDD.Exceptions;
#pragma warning disable CA1032
public class ConcurrencyException : Exception
{
    public readonly string AggregateId;
    public readonly int ExpectedVersion;
    public readonly int VersionFound;

    public ConcurrencyException(string aggregateId, int expectedVersion, int versionFound, Exception? innerException = null) : base(
        $"There was an attempt to save multiple, concurrent changes to an aggregate that could not be resolved. ({aggregateId})", innerException)
    {
        if (string.IsNullOrEmpty(aggregateId)) throw new ArgumentException("You cannot create a ConcurrencyException without an AggregateId", nameof(aggregateId));

        if (expectedVersion == versionFound) throw new ArgumentException("You cannot create a ConcurrencyException with matching expected and found version values.", nameof(versionFound));

        AggregateId = aggregateId;
        ExpectedVersion = expectedVersion;
        VersionFound = versionFound;
    }
}