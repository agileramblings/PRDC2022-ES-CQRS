namespace DepthConsulting.Core.Messaging;

public class Correlation
{
    public Correlation(string? correlationId = null)
    {
        if (string.IsNullOrEmpty(correlationId)) correlationId = Guid.NewGuid().ToString();
        CorrelationId = correlationId;
    }

    public string? CorrelationId { get; set; }
}