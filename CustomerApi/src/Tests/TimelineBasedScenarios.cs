using Kekiri.Xunit;
using PRDC2022.Customer.Domain.Aggregates.Timeline;
using PRDC2022.Customer.Domain.Aggregates.Timeline.DomainEvents;

namespace PRDC2022.Customer.Tests;

public class TimelineCreationScenarios : Scenarios
{
    private const string CreatedBy = "DaveW";
    private const string TimeLineId = "Test_Customer_Id";

    private const string FirstName = "Santa";
    private const string LastName = "Claus";
    private const string MiddleName = "M";
    private const string Gender = "RatherNotSay";
    private const string Email = "santa@christmas.com";
    private const string ModifiedBy = "DylanS";

    private const string Address1 = "123 Canada Ave";
    private const string Address2 = "Elves Workshop";
    private const string City = "North Pole";
    private const string Region = "Nunavut";
    private const string Country = "Canada";
    private const string PostalCode = "H0H 0H0";
    private const string AddedBy = "D'arcyL";
    private const string MadePrimaryBy = "SimonT";
    private readonly DateTime _addedOn = DateTime.Now.AddDays(2);
    private readonly DateTime _createdOn = DateTime.Now;
    private readonly DateTime _dateOfBirth = new(1776, 1, 1);

    private readonly DateTime _madePrimaryOn = DateTime.Now.AddDays(4);
    private readonly DateTime _modifiedOn = DateTime.Now.AddDays(1);
    private Timeline _sut = new();
    
    [Scenario]
    public void Timeline_Created()
    {
        const string correlationId = nameof(Timeline_Created);
        Given(A_Null_Timeline_Instance, correlationId);
        When(A_Timeline_Is_Created, correlationId);
        Then(The_ID_Property_Should_Be_Set)
            .And(The_Aggregate_Has_1_Event)
            .And(The_event_is_a_TimelineCreated_event);
    }

    private void A_Null_Timeline_Instance(string correlationId)
    {
        _sut = null;
    }

    private void A_Timeline_Is_Created(string correlationId)
    {
        _sut = new Timeline(TimeLineId);
    }

    private void The_ID_Property_Should_Be_Set()
    {
       Assert.Equal(TimeLineId, _sut.AggregateId);
    }

    private void The_Aggregate_Has_1_Event()
    {
        Assert.Equal(1, _sut.EventCount);
    }

    private void The_event_is_a_TimelineCreated_event()
    {
        Assert.IsType<TimelineCreated>(_sut.GetUncommittedChanges().First());
    }

    [Scenario]
    public void A_Timeline_has_an_event_added()
    {
        const string correlationId = nameof(A_Timeline_has_an_event_added);
        Given(A_Timeline_With_A_Created_Event, correlationId);
        When(A_Timeline_Has_An_Event_Added, correlationId);
        Then(The_Aggregate_Has_2_Events)
            .And(The_event_is_a_TimelineCreated_event)
            .And(The_event_is_a_TimelineEventAdded_event);
    }

    private void A_Timeline_With_A_Created_Event(string obj)
    {
        throw new NotImplementedException();
    }

    private void The_Aggregate_Has_2_Events()
    {
        throw new NotImplementedException();
    }

    private void The_event_is_a_TimelineEventAdded_event()
    {
        throw new NotImplementedException();
    }

    private void A_Timeline_Has_An_Event_Added(string obj)
    {
        _sut.AddEvent("Test Event", "Test Description", "Test Location", DateTime.Now.AddDays(1), "Test Type", "Test Status", "Test Notes", "Test Created By");
    }
}