namespace PRDC2022.Customer.Domain.Exceptions;

public class AddressAlreadyExistsException : Exception
{
    public AddressAlreadyExistsException(string aggregateId, Guid addressId)
    {
        AggregateId = aggregateId;
        AddressId = addressId;
    }

    public Guid AddressId { get; init; }

    public string AggregateId { get; init; }
}