namespace PRDC2022.Customer.Domain.Exceptions;

public class PhoneNotFoundException : Exception
{
    public PhoneNotFoundException(string aggregateId, Guid phoneId)
    {
        AggregateId = aggregateId;
        PhoneId = phoneId;
    }

    public Guid PhoneId { get; init; }

    public string AggregateId { get; init; }
}