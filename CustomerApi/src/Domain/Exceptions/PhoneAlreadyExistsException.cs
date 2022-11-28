namespace PRDC2022.Customer.Domain.Exceptions;

public class PhoneAlreadyExistsException : Exception
{
    public PhoneAlreadyExistsException(string aggregateId, Guid phoneId)
    {
        AggregateId = aggregateId;
        PhoneId = phoneId;
    }

    public Guid PhoneId { get; init; }

    public string AggregateId { get; init; }
}