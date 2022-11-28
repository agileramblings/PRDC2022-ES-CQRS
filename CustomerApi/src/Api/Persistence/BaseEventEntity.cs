using System.ComponentModel.DataAnnotations;

namespace PRDC2022.CustomerApi.Persistence
{
    public class BaseEventEntity
    {
        // composite key set in model creation on DbContext
        // Composite key is AggregateId + Version + AggregateType
        [Required] public Guid MessageId { get; set; }
        [Required] public string CorrelationId { get; set; }
        [Required] public Guid CausationId { get; set; }

        [Required] [MaxLength(50)] public string AggregateId { get; set; }

        [Required] public int Version { get; set; }
        [Required] public string EventType { get; set; }
        [Required] public string EventData { get; set; }
        [Required] public DateTime ReceivedOn { get; set; }

        [Required] [MaxLength(120)] public string AggregateType { get; set; }
    }
}