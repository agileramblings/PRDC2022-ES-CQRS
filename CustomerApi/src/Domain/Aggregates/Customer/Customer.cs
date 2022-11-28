using DepthConsulting.Core.DDD;
using PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;
using PRDC2022.Customer.Domain.Exceptions;
using PRDC2022.Customer.Domain.Projections;
using PRDC2022.Customer.Domain.Shared;

namespace PRDC2022.Customer.Domain.Aggregates.Customer;

public class Customer : AggregateBase
{
    public Customer() : base("tbd")
    {
        // DO NOT USE - Required by serializers
    }

    public Customer(string aggregateId, DateTime createdOn, string correlationId, Guid causationId,
        string createdBy) : base("tbd")
    {
        ApplyChange(new CustomerCreated(aggregateId, createdOn, correlationId, causationId, createdBy));
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? MiddleName { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string? Gender { get; private set; }
    public Dictionary<Guid, Address> Addresses { get; } = new();
    public Dictionary<Guid, Phone> PhoneRecords { get; } = new();
    public bool Active { get; set; }

    public void UpdateBasicDetails(BasicDetailsRequestParams details, DateTime updatedOn, string correlationId,
        Guid causationId, string updatedBy)
    {
        if (!Equals(details)) // some basic detail will be changed
            ApplyChange(new BasicsUpdated(details, AggregateId, updatedOn, correlationId, causationId, updatedBy));
    }

    public Guid AddAddress(AddressDetailsRequestParams details, DateTime addedOn, string correlationId,
        Guid causationId, string addedBy)
    {
        var address = Addresses.Select(kvp => kvp.Value).FirstOrDefault(a => a.Equivalent(details));
        if (address != null)
            // A similar Address already exists.. it cannot be added again
            throw new AddressAlreadyExistsException(AggregateId, address.AddressId);
        var newPhoneId = Guid.NewGuid();
        ApplyChange(new AddressAdded(details, newPhoneId, AggregateId, addedOn, correlationId, causationId, addedBy));
        return newPhoneId;
    }

    public Guid AddPhoneRecord(PhoneDetailsRequestParams details, DateTime addedOn, string correlationId,
        Guid causationId, string addedBy)
    {
        var phoneRecord = PhoneRecords.Select(kvp => kvp.Value).FirstOrDefault(a => a.Equivalent(details));
        if (phoneRecord != null)
            // A similar Phone Record already exists.. it cannot be added again
            throw new PhoneAlreadyExistsException(AggregateId, phoneRecord.PhoneId);
        var newAddressId = Guid.NewGuid();
        ApplyChange(new PhoneAdded(details, newAddressId, AggregateId, addedOn, correlationId, causationId, addedBy));
        return newAddressId;
    }

    public void MakeAddressPrimaryBilling(Guid addressId, DateTime modifiedOn, string correlationId, Guid causationId,
        string modifiedBy)
    {
        _ = GetAddress(addressId);
        ApplyChange(new AddressMadePrimaryBilling(addressId, AggregateId, modifiedOn, correlationId, causationId,
            modifiedBy));
    }


    public void MakeAddressPrimaryShipping(Guid addressId, DateTime modifiedOn, string correlationId, Guid causationId,
        string modifiedBy)
    {
        _ = GetAddress(addressId);
        ApplyChange(new AddressMadePrimaryShipping(addressId, AggregateId, modifiedOn, correlationId, causationId,
            modifiedBy));
    }

    public void UpdateAddress(Guid addressId, AddressDetailsRequestParams addressDetailsRequestParams,
        DateTime updatedOn, string correlationId, Guid causationId, string updatedBy)
    {
        _ = GetAddress(addressId);
        ApplyChange(new AddressUpdated(addressDetailsRequestParams, addressId, AggregateId, updatedOn, correlationId,
            causationId, updatedBy));
    }

    public void UpdatePhoneRecord(Guid phoneId, PhoneDetailsRequestParams phoneDetailsRequestParams, DateTime updatedOn,
        string correlationId, Guid causationId, string updatedBy)
    {
        _ = GetPhoneRecord(phoneId);
        ApplyChange(new PhoneUpdated(phoneDetailsRequestParams, phoneId, AggregateId, updatedOn, correlationId,
            causationId, updatedBy));
    }

    public void MakePhonePrimary(Guid phoneId, DateTime modifiedOn, string correlationId, Guid causationId,
        string modifiedBy)
    {
        _ = GetPhoneRecord(phoneId);
        ApplyChange(new PhoneMadePrimary(phoneId, AggregateId, modifiedOn, correlationId, causationId, modifiedBy));
    }

    public void RemoveAddress(Guid addressId, DateTime removedOn, string correlationId, Guid causationId,
        string removedBy)
    {
        _ = GetAddress(addressId);
        ApplyChange(new AddressRemoved(addressId, AggregateId, removedOn, correlationId, causationId, removedBy));
    }

    public void RemovePhoneRecord(Guid phoneId, DateTime removedOn, string correlationId, Guid causationId,
        string removedBy)
    {
        _ = GetPhoneRecord(phoneId);
        ApplyChange(new PhoneRemoved(phoneId, AggregateId, removedOn, correlationId, causationId, removedBy));
    }

    public void RemoveCustomer(DateTime removedOn, string correlationId, Guid causationId, string removedBy)
    {
        ApplyChange(new CustomerRemoved(AggregateId, removedOn, correlationId, causationId, removedBy));
    }

    public CustomerDetails ToDetails()
    {
        return new CustomerDetails(AggregateId)
        {
            Version = Version,
            CreatedBy = CreatedBy,
            CreatedOn = Created,
            ModifiedBy = ModifiedBy,
            LastModifiedOn = Modified,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Gender = Gender,
            DateOfBirth = DateOfBirth,
            Email = Email,
            PhoneRecords = PhoneRecords.Select(p => p.Value).ToList(),
            Addresses = Addresses.Select(a => a.Value).ToList()
        };
    }

#pragma warning disable IDE0051 // Remove unused private members
    // ReSharper disable UnusedMember.Local

    private void Apply(CustomerCreated @event)
    {
        AggregateId = @event.AggregateId;
        CreatedBy = @event.AttributedTo;
        Created = @event.ReceivedOn;
    }

    private void Apply(CustomerRemoved @event)
    {
        Active = false;
        ModifiedBy = @event.AttributedTo;
        Modified = @event.ReceivedOn;
    }

    private void Apply(BasicsUpdated @event)
    {
        FirstName = @event.Details.FirstName;
        LastName = @event.Details.LastName;
        MiddleName = @event.Details.MiddleName;
        Gender = @event.Details.Gender;
        Email = @event.Details.Email;
        DateOfBirth = @event.Details.DateOfBirth;
        ModifiedBy = @event.AttributedTo;
        Modified = @event.ReceivedOn;
    }

    private void Apply(AddressAdded @event)
    {
        if (Addresses.ContainsKey(@event.AddressId))
            throw new AddressAlreadyExistsException(AggregateId, @event.AddressId);
        Addresses.Add(@event.AddressId, new Address
        {
            AddressId = @event.AddressId,
            Address1 = @event.Details.Address1,
            Address2 = @event.Details.Address2,
            City = @event.Details.City,
            Country = @event.Details.Country,
            PostalCode = @event.Details.PostalCode,
            Region = @event.Details.Region
        });
        ModifiedBy = @event.AttributedTo;
        Modified = @event.ReceivedOn;
    }

    private void Apply(AddressUpdated @event)
    {
        if (!Addresses.ContainsKey(@event.AddressId))
            throw new AddressNotFoundException(AggregateId, @event.AddressId);

        Addresses[@event.AddressId].Address1 = @event.Details.Address1;
        Addresses[@event.AddressId].Address2 = @event.Details.Address2;
        Addresses[@event.AddressId].City = @event.Details.City;
        Addresses[@event.AddressId].Country = @event.Details.Country;
        Addresses[@event.AddressId].PostalCode = @event.Details.PostalCode;
        Addresses[@event.AddressId].Region = @event.Details.Region;

        ModifiedBy = @event.AttributedTo;
        Modified = @event.ReceivedOn;
    }

    private void Apply(PhoneAdded @event)
    {
        if (PhoneRecords.ContainsKey(@event.PhoneId))
            throw new PhoneAlreadyExistsException(AggregateId, @event.PhoneId);
        PhoneRecords.Add(@event.PhoneId, new Phone
        {
            PhoneId = @event.PhoneId,
            Number = @event.Details.Number,
            CountryCode = @event.Details.CountryCode,
            PhoneType = @event.Details.PhoneType
        });
        ModifiedBy = @event.AttributedTo;
        Modified = @event.ReceivedOn;
    }

    private void Apply(PhoneUpdated @event)
    {
        if (!PhoneRecords.ContainsKey(@event.PhoneId))
            throw new PhoneNotFoundException(AggregateId, @event.PhoneId);

        PhoneRecords[@event.PhoneId].Number = @event.Details.Number;
        PhoneRecords[@event.PhoneId].CountryCode = @event.Details.CountryCode;
        PhoneRecords[@event.PhoneId].PhoneType = @event.Details.PhoneType;

        ModifiedBy = @event.AttributedTo;
        Modified = @event.ReceivedOn;
    }

    private void Apply(AddressMadePrimaryBilling @event)
    {
        var hasExistingRecord = Addresses.Any(p => p.Value.IsPrimaryBilling);
        if (hasExistingRecord)
        {
            var existingRecord = Addresses.FirstOrDefault(p => p.Value.IsPrimaryBilling).Value;
            if (existingRecord.AddressId == @event.AddressId)
            {
                return; // nothing to do here
            }

            Addresses[existingRecord.AddressId].IsPrimaryBilling = false;
        }

        // replace existing record with new record with flag set to true
        var address = GetAddress(@event.AddressId);
        Addresses[address.AddressId].IsPrimaryBilling = true;

        ModifiedBy = @event.AttributedTo;
        Modified = @event.ReceivedOn;
    }

    private void Apply(AddressMadePrimaryShipping @event)
    {
        var hasExistingRecord = Addresses.Any(p => p.Value.IsPrimaryShipping);
        if (hasExistingRecord)
        {
            var existingRecord = Addresses.FirstOrDefault(p => p.Value.IsPrimaryShipping).Value;
            if (existingRecord.AddressId == @event.AddressId)
            {
                return; // nothing to do here
            }

            Addresses[existingRecord.AddressId].IsPrimaryShipping = false;
        }

        // replace existing record with new record with flag set to true
        var address = GetAddress(@event.AddressId);
        Addresses[address.AddressId].IsPrimaryShipping = true;

        ModifiedBy = @event.AttributedTo;
        Modified = @event.ReceivedOn;
    }

    private void Apply(PhoneMadePrimary @event)
    {
        var hasExistingRecord = PhoneRecords.Any(p => p.Value.IsPrimary);
        if (hasExistingRecord)
        {
            var existingRecord = PhoneRecords.FirstOrDefault(p => p.Value.IsPrimary).Value;
            if (existingRecord.PhoneId == @event.PhoneId)
            {
                return; // nothing to do here
            }

            PhoneRecords[existingRecord.PhoneId].IsPrimary = false;
        }

        // replace existing record with new record with flag set to true
        var phoneRecord = GetPhoneRecord(@event.PhoneId);
        PhoneRecords[phoneRecord.PhoneId].IsPrimary = true;
        ModifiedBy = @event.AttributedTo;
        Modified = @event.ReceivedOn;
    }

    private void Apply(AddressRemoved @event)
    {
        try
        {
            var address = GetAddress(@event.AddressId);
            Addresses.Remove(address.AddressId);
            ModifiedBy = @event.AttributedTo;
            Modified = @event.ReceivedOn;
        }
        catch (AddressNotFoundException)
        {
            // do nothing, address already removed from public collection
        }
    }

    private void Apply(PhoneRemoved @event)
    {
        try
        {
            var phoneRecord = GetPhoneRecord(@event.PhoneId);
            PhoneRecords.Remove(phoneRecord.PhoneId);
            ModifiedBy = @event.AttributedTo;
            Modified = @event.ReceivedOn;
        }
        catch (PhoneNotFoundException)
        {
            // do nothing, phone already removed from public collection
        }
    }
#pragma warning restore IDE0051 // Remove unused private members

    private Address GetAddress(Guid addressId)
    {
        if (!Addresses.ContainsKey(addressId)) throw new AddressNotFoundException(AggregateId, addressId);

        return Addresses[addressId];
    }

    private Phone GetPhoneRecord(Guid phoneId)
    {
        if (!PhoneRecords.ContainsKey(phoneId)) throw new PhoneNotFoundException(AggregateId, phoneId);

        return PhoneRecords[phoneId];
    }

    private bool Equals(BasicDetailsRequestParams details)
    {
        return FirstName == details.FirstName &&
               LastName == details.LastName &&
               MiddleName == details.MiddleName &&
               DateOfBirth == details.DateOfBirth &&
               Email == details.Email &&
               Gender == details.Gender;
    }
}