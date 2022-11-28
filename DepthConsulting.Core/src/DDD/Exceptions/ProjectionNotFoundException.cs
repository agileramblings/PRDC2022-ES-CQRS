namespace DepthConsulting.Core.DDD.Exceptions;

public class ProjectionNotFoundException : Exception
{
    public readonly string ProjectionId;

    public ProjectionNotFoundException(string projectionId, Exception? innerException = null) : base(
        $"There were no projection available for Id: ({projectionId})", innerException)
    {
        if (string.IsNullOrEmpty(projectionId)) throw new ArgumentException("You cannot create a ProjectionNotFoundException without an Projection Id", nameof(projectionId));

        ProjectionId = projectionId;
    }
}