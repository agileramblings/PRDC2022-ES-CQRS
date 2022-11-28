namespace DepthConsulting.Core.DDD;

public abstract class ReadModelBase
{
}

public abstract class ProjectionBase : ReadModelBase
{
    /// <summary>
    ///     Aggregate-based Identity
    /// </summary>
    /// <param name="aggregateId"></param>
    protected ProjectionBase(string aggregateId)
    {
        AggregateId = aggregateId;
    }
#pragma warning disable IDE1006
    // ReSharper disable once InconsistentNaming
    // This must stay lower-case for the CosmosDb ReadModel to work
    public string id
    {
        get => AggregateId;
        private set => AggregateId = value;
    }

    public string AggregateId { get; set; }
    public int Version { get; set; }
}

public abstract class TypeBasedProjectionBase : ReadModelBase
{
    /// <summary>
    ///     Type-based identity
    /// </summary>

    // ReSharper disable once InconsistentNaming
    public string id // This must stay lower-case for the CosmosDb ReadModel to work
    {
        get => GetType().Name;
        set => _ = 0; //noop
    }
}