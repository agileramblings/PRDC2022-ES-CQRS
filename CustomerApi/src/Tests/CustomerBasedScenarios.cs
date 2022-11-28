using Kekiri.Xunit;
using PRDC2022.Customer.Domain.Aggregates.Customer.DomainEvents;
using PRDC2022.Customer.Domain.Exceptions;
using PRDC2022.Customer.Domain.Shared;

namespace PRDC2022.Customer.Tests;

public class CustomerCreationScenarios : Scenarios
{
    private const string CreatedBy = "DaveW";
    private const string CustomerId = "Test_Customer_Id";

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
    private Domain.Aggregates.Customer.Customer _sut = new();
    
    [Scenario]
    public void Customer_Creation()
    {
        const string correlationId = nameof(Customer_Creation);
        Given(A_Null_Customer_Instance, correlationId);
        When(A_Customer_Is_Created, correlationId);
        Then(The_ID_Property_Should_Be_Set)
            .And(The_Aggregate_Has_1_Event)
            .And(The_event_is_a_CustomerCreated_event);
    }

    private void A_Null_Customer_Instance(string correlationId)
    {
        _sut = null;
    }

    private void A_Customer_Is_Created(string correlationId)
    {
        _sut = new Domain.Aggregates.Customer.Customer(CustomerId, _createdOn, correlationId, Guid.Empty, CreatedBy);
    }

    private void The_ID_Property_Should_Be_Set()
    {
        Assert.Equal(CustomerId, _sut.AggregateId);
    }

    private void The_Aggregate_Has_1_Event()
    {
        Assert.Single(_sut.GetUncommittedChanges());
    }
    private void The_event_is_a_CustomerCreated_event()
    {
        Assert.IsType<CustomerCreated>(_sut.GetUncommittedChanges().First());
    }

    [Scenario]
    public void Customer_Basics_Updated()
    {
        const string correlationId = nameof(Customer_Basics_Updated);
        Given(A_Customer_Is_Created, correlationId);
        When(The_Customer_Basics_Are_Updated, correlationId);
        Then(The_customer_details_should_be_updated)
            .And(The_Aggregate_Has_2_Events)
            .And(The_events_are_the_right_types);
    }

    private void The_Customer_Basics_Are_Updated(string correlationId)
    {
        var basicReqParam = new BasicDetailsRequestParams
        {
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Gender = Gender,
            DateOfBirth = _dateOfBirth,
            Email = Email
        };
        _sut.UpdateBasicDetails(basicReqParam, _modifiedOn, correlationId, Guid.Empty, ModifiedBy);
    }

    private void The_customer_details_should_be_updated()
    {
        Assert.Equal(FirstName, _sut.FirstName);
        Assert.Equal(LastName, _sut.LastName);
        Assert.Equal(MiddleName, _sut.MiddleName);
        Assert.Equal(Gender, _sut.Gender);
        Assert.Equal(_dateOfBirth, _sut.DateOfBirth);
        Assert.Equal(Email, _sut.Email);
    }

    private void The_Aggregate_Has_2_Events()
    {
        Assert.Equal(2, _sut.GetUncommittedChanges().Count());
    }
    private void The_events_are_the_right_types()
    {
        Assert.IsType<CustomerCreated>(_sut.GetUncommittedChanges().First());
        Assert.IsType<BasicsUpdated>(_sut.GetUncommittedChanges().Last());
    }


    [Scenario]
    public void Customer_Has_Address_Added()
    {
        const string correlationId = nameof(Customer_Has_Address_Added);
        Given(A_Customer_Is_Created, correlationId)
            .And(The_Customer_Basics_Are_Updated, correlationId);
        When(An_Address_is_added, correlationId);
        Then(The_address_in_the_collection_is_correct)
            .And(The_address_collection_has_1_address)
            .And(The_aggregate_has_3_events);
    }

    private void An_Address_is_added(string correlationId)
    {
        var addReqParams = new AddressDetailsRequestParams
        {
            Address1 = Address1,
            Address2 = Address2,
            City = City,
            Region = Region,
            Country = Country,
            PostalCode = PostalCode
        };
        _sut.AddAddress(addReqParams, _addedOn, correlationId, Guid.Empty, AddedBy);
    }

    private void The_address_in_the_collection_is_correct()
    {
        var address = _sut.Addresses.First().Value;
        Assert.Equal(Address1, address?.Address1);
        Assert.Equal(Address2, address?.Address2);
        Assert.Equal(City, address?.City);
        Assert.Equal(Region, address?.Region);
        Assert.Equal(Country, address?.Country);
        Assert.Equal(PostalCode, address?.PostalCode);
    }

    private void The_address_collection_has_1_address()
    {
        Assert.Single(_sut.Addresses);
    }

    private void The_aggregate_has_3_events()
    {
        Assert.Equal(3, _sut.GetUncommittedChanges().Count());
    }

    [Scenario]
    public void An_Address_Is_Made_Primary()
    {
        const string correlationId = nameof(An_Address_Is_Made_Primary);
        Given(A_Customer_Is_Created, correlationId)
            .And(The_Customer_Basics_Are_Updated, correlationId)
            .And(An_Address_is_added, correlationId);
        When(The_address_is_made_billing_primary, correlationId);
        Then(The_single_address_is_primary_in_the_aggregate)
            .And(There_are_5_events_in_the_aggregate);
    }

    private void The_address_is_made_billing_primary(string correlationId)
    {
        var address = _sut.Addresses.First().Value;
        _sut.MakeAddressPrimaryBilling(address.AddressId, _madePrimaryOn, correlationId, Guid.Empty, MadePrimaryBy);
        _sut.MakeAddressPrimaryShipping(address.AddressId, _madePrimaryOn, correlationId, Guid.Empty, MadePrimaryBy);
    }

    private void The_single_address_is_primary_in_the_aggregate()
    {
        Assert.Equal(true, _sut.Addresses.First().Value.IsPrimaryBilling);
        Assert.Equal(true, _sut.Addresses.First().Value.IsPrimaryShipping);
    }

    private void There_are_5_events_in_the_aggregate()
    {
        Assert.Equal(5, _sut.GetUncommittedChanges().Count());
    }

    [Scenario]
    public void Add_Two_Addresses_To_A_Customer_And_Make_Them_Separate_Primaries()
    {
        const string correlationId = nameof(Add_Two_Addresses_To_A_Customer_And_Make_Them_Separate_Primaries);
        Given(A_Customer_Is_Created, correlationId)
            .And(The_Customer_Basics_Are_Updated, correlationId)
            .And(An_Address_is_added, correlationId)
            .And(A_second_address_is_added, correlationId);
        When(The_addresses_are_made_into_separate_primaries, correlationId);
        Then(The_addresses_have_the_correct_attributes)
            .And(There_are_6_events_in_the_aggregate);
    } // ReSharper disable InconsistentNaming
    private const string AddressTwo_Address1 = "123 South Pole Blvd";
    private const string AddressTwo_Address2 = "Penguin Pirate Cave";
    private const string AddressTwo_City = "Casa De La Muerta";
    private const string AddressTwo_Region = "South Pole";
    private const string AddressTwo_Country = "Anarctica";
    private const string AddressTwo_PostalCode = "P1P 1P1";

    private void A_second_address_is_added(string correlationId)
    {
        var addReqParams = new AddressDetailsRequestParams
        {
            Address1 = AddressTwo_Address1,
            Address2 = AddressTwo_Address2,
            City = AddressTwo_City,
            Region = AddressTwo_Region,
            Country = AddressTwo_Country,
            PostalCode = AddressTwo_PostalCode
        };
        _sut.AddAddress(addReqParams, _addedOn, correlationId, Guid.Empty, AddedBy);
    }

    private void The_addresses_are_made_into_separate_primaries(string correlationId)
    {
        var address = _sut.Addresses.First().Value;
        _sut.MakeAddressPrimaryBilling(address.AddressId, _madePrimaryOn, correlationId, Guid.Empty, MadePrimaryBy);
        address = _sut.Addresses.Last().Value;
        _sut.MakeAddressPrimaryShipping(address.AddressId, _madePrimaryOn, correlationId, Guid.Empty, MadePrimaryBy);
    }

    private void The_addresses_have_the_correct_attributes()
    {
        Assert.True(_sut.Addresses.First().Value.IsPrimaryBilling);
        Assert.True(_sut.Addresses.Last().Value.IsPrimaryShipping);
    }

    private void There_are_6_events_in_the_aggregate()
    {
        Assert.Equal(6, _sut.GetUncommittedChanges().Count());
    }

    [Scenario]
    public void There_Can_Only_Be_One_Address_With_A_Particular_Primary_Flag_At_A_Time()
    {
        const string correlationId = nameof(Add_Two_Addresses_To_A_Customer_And_Make_Them_Separate_Primaries);
        Given(A_Customer_Is_Created, correlationId)
            .And(The_Customer_Basics_Are_Updated, correlationId)
            .And(An_Address_is_added, correlationId)
            .And(A_second_address_is_added, correlationId)
            .And(And_first_address_is_made_primary, correlationId);
        When(The_second_address_is_made_billing_primary, correlationId);
        Then(The_first_address_has_only_shipping_primary_flag)
            .And(The_second_address_has_only_primary_billing_flag);
    }

    private void And_first_address_is_made_primary(string correlationId)
    {
        var address = _sut.Addresses.First().Value;
        _sut.MakeAddressPrimaryShipping(address.AddressId, _madePrimaryOn, correlationId, Guid.Empty, MadePrimaryBy);
        _sut.MakeAddressPrimaryBilling(address.AddressId, _madePrimaryOn, correlationId, Guid.Empty, MadePrimaryBy);
    }

    private void The_second_address_is_made_billing_primary(string correlationId)
    {
        var address = _sut.Addresses.Last().Value;
        _sut.MakeAddressPrimaryBilling(address.AddressId, _madePrimaryOn, correlationId, Guid.Empty, MadePrimaryBy);
    }

    private void The_first_address_has_only_shipping_primary_flag()
    {
        Assert.True(_sut.Addresses.First().Value.IsPrimaryShipping);
        Assert.False(_sut.Addresses.First().Value.IsPrimaryBilling);
    }

    private void The_second_address_has_only_primary_billing_flag()
    {
        Assert.False(_sut.Addresses.Last().Value.IsPrimaryShipping);
        Assert.True(_sut.Addresses.Last().Value.IsPrimaryBilling);
    }

    [Scenario]
    public void A_PhoneNumber_Is_Added()
    {
        const string correlationId = nameof(A_PhoneNumber_Is_Added);
        Given(A_Customer_Is_Created, correlationId)
            .And(The_Customer_Basics_Are_Updated, correlationId);
        When(A_phone_number_is_added, correlationId);
        Then(The_customer_has_1_phone_number)
            .And(The_aggregate_has_3_events);
    }

    private Guid PhoneOne_PhoneId;
    private const int CountryCode = 1;
    private const long Number = 5558675309;
    private const string PhoneType = "Cellular";
    private readonly DateTime _phoneAddedOn = DateTime.Now.AddDays(6);
    private const string PhoneAddedBy = "EricF";

    private void A_phone_number_is_added(string correlationId)
    {
        PhoneOne_PhoneId  = _sut.AddPhoneRecord(new PhoneDetailsRequestParams
        {
            CountryCode = CountryCode,
            Number = Number,
            PhoneType = PhoneType
        }, _phoneAddedOn, correlationId, Guid.Empty, PhoneAddedBy);
    }

    private void The_customer_has_1_phone_number()
    {
        Assert.Equal(1, _sut.PhoneRecords.Count);
    }

    [Scenario]
    public void Two_PhoneNumbers_Are_Added_And_One_is_Primary()
    {
        const string correlationId = nameof(Two_PhoneNumbers_Are_Added_And_One_is_Primary);
        Given(A_Customer_Is_Created, correlationId)
            .And(The_Customer_Basics_Are_Updated, correlationId)
            .And(A_phone_number_is_added, correlationId)
            .And(A_second_phone_number_is_added, correlationId);
        When(The_first_phone_number_is_made_primary, correlationId);
        Then(The_customer_has_2_phone_number)
            .And(The_first_phone_number_is_primary);
    }

    private Guid PhoneTwo_PhoneId;
    private const long PhoneTwo_Number = 6668675309;

    private void A_second_phone_number_is_added(string correlationId)
    {
        PhoneTwo_PhoneId = _sut.AddPhoneRecord(new PhoneDetailsRequestParams
        {
            CountryCode = CountryCode,
            Number = PhoneTwo_Number,
            PhoneType = PhoneType
        }, _phoneAddedOn, correlationId, Guid.Empty, PhoneAddedBy);
    }

    private void The_first_phone_number_is_made_primary(string correlationId)
    {
        var phoneRecord = _sut.PhoneRecords.First().Value;
        _sut.MakePhonePrimary(PhoneOne_PhoneId, _phoneAddedOn, correlationId, Guid.Empty, PhoneAddedBy);
    }

    private void The_customer_has_2_phone_number()
    {
        Assert.Equal(2, _sut.PhoneRecords.Count);
    }

    private void The_first_phone_number_is_primary()
    {
        Assert.True(_sut.PhoneRecords.First(pr => pr.Key == PhoneOne_PhoneId).Value.IsPrimary);
    }

    [Scenario]
    public void Two_PhoneNumbers_Are_Added_And_The_Primary_Is_Changed()
    {
        const string correlationId = nameof(Two_PhoneNumbers_Are_Added_And_The_Primary_Is_Changed);
        Given(A_Customer_Is_Created, correlationId)
            .And(The_Customer_Basics_Are_Updated, correlationId)
            .And(A_phone_number_is_added, correlationId)
            .And(A_second_phone_number_is_added, correlationId)
            .And(The_first_phone_number_is_made_primary, correlationId);
        When(The_second_phone_number_is_made_primary, correlationId);
        Then(The_customer_has_2_phone_number)
            .And(The_first_phone_number_is_not_primary)
            .And(The_second_phone_number_is_primary);
    }

    private void The_second_phone_number_is_made_primary(string correlationId)
    {
        _sut.MakePhonePrimary(PhoneTwo_PhoneId, _madePrimaryOn, correlationId, Guid.Empty, MadePrimaryBy);
    }

    private void The_first_phone_number_is_not_primary()
    {
        Assert.False(_sut.PhoneRecords.First(pr => pr.Key == PhoneOne_PhoneId).Value.IsPrimary);
    }

    private void The_second_phone_number_is_primary()
    {
        Assert.True(_sut.PhoneRecords.Last().Value.IsPrimary);
    }

    [Scenario]
    public void Attempting_to_add_Equivalent_PhoneNumbers_Throws_An_Exception()
    {
        const string correlationId = nameof(Attempting_to_add_Equivalent_PhoneNumbers_Throws_An_Exception);
        Given(A_Customer_Is_Created, correlationId)
            .And(The_Customer_Basics_Are_Updated, correlationId)
            .And(A_phone_number_is_added, correlationId);
        When(A_second_equivalent_phone_number_is_added, correlationId).Throws();
        Then(A_phone_already_exists_exception_is_caught);
    }

    private void A_phone_already_exists_exception_is_caught()
    {
        Catch<PhoneAlreadyExistsException>();
    }

    private void A_second_equivalent_phone_number_is_added(string correlationId)
    {
        _ = _sut.AddPhoneRecord(new PhoneDetailsRequestParams
        {
            CountryCode = CountryCode,
            Number = Number,
            PhoneType = PhoneType
        }, _phoneAddedOn, correlationId, Guid.Empty, PhoneAddedBy);
    }


    [Scenario]
    public void Attempting_to_add_Equivalent_Address_Throws_An_Exception()
    {
        const string correlationId = nameof(Attempting_to_add_Equivalent_Address_Throws_An_Exception);
        Given(A_Customer_Is_Created, correlationId)
            .And(The_Customer_Basics_Are_Updated, correlationId)
            .And(An_Address_is_added, correlationId);
        When(An_Address_is_added, correlationId).Throws();
        Then(A_address_already_exists_exception_is_caught);
    }

    private void A_address_already_exists_exception_is_caught()
    {
        Catch<AddressAlreadyExistsException>();
    }
}