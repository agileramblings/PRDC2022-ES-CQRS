using DepthConsulting.Core.DDD;
using PRDC2022.Customer.Domain.Aggregates.Timeline.DomainEvents;

namespace PRDC2022.Customer.Domain.Aggregates.Timeline
{
    public class Timeline :AggregateBase
    {
        public Timeline() : base("tbd")
        {
            // DO NOT USE - Required by serializers
        }


        public Timeline(string testCustomerId) : base(testCustomerId)
        {
            ApplyChange(new TimelineCreated(testCustomerId, DateTime.Now, "tbd", new System.Guid(), "tbd"));
        }

        public void AddEvent(string testEvent, string testDescription, string testLocation, DateTime addDays, string testType, string testStatus, string testNotes, string testCreatedBy)
        {
            throw new NotImplementedException();
        }

        // private methods
        private void Apply(TimelineCreated e)
        {
            AggregateId = e.AggregateId;
            Created = e.ReceivedOn;
            CreatedBy = e.AttributedTo;
        }
    }
}
