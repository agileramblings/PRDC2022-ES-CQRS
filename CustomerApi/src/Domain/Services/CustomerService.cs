using System.Data;
using Dapper;
using DepthConsulting.Core.Services.Persistence.CQRS;
using DepthConsulting.Core.Services.Persistence.EventSource.Aggregates;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using PRDC2022.Customer.Domain.Projections;
using PRDC2022.Customer.Domain.Shared;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace PRDC2022.Customer.Domain.Services;

public interface ICustomerService
{
    Task<CustomerDetails> CreateCustomer(Guid newCustomerId, Instant createdOn, BasicDetailsRequestParams basicDetailsRequestParams);
    Task<IEnumerable<CustomerDetails>?> GetAllCustomers();
    Task<CustomerDetails?> GetCustomer(string id);
    Task<CustomerDetails?> UpdateBasics(string id, BasicDetailsRequestParams basicDetailsRequestParams, Instant getCurrentInstant);
    Task<CustomerDetails?> AddAddress(string id, AddressDetailsRequestParams addressDetailsRequestParams, Instant getCurrentInstant);
    Task<CustomerDetails?> AddPhoneNumber(string id, PhoneDetailsRequestParams phoneDetailsRequestParams, Instant getCurrentInstant);
    Task<CustomerDetails?> UpdateAddress(string id, Guid addressId, AddressDetailsRequestParams addressDetailsRequestParams, Instant updatedOn);
    Task<CustomerDetails?> MakeAddressPrimaryShipping(string id, Guid addressId, Instant madePrimaryOn);
    Task<CustomerDetails?> MakeAddressPrimaryBilling(string id, Guid addressId, Instant madeBillingOn);
    Task<CustomerDetails?> UpdatePhone(string id, Guid phoneId, PhoneDetailsRequestParams addressDetailsRequestParams, Instant updatedOn);
    Task<CustomerDetails?> MakePhonePrimary(string id, Guid phoneId, Instant madePrimaryOn);
    Task<bool> RemoveCustomer(string id, Instant removedOn);
    Task<CustomerDetails?> RemoveAddress(string id, Guid addressId, Instant removedOn);
    Task<CustomerDetails?> RemovePhone(string id, Guid phoneId, Instant removedOn);
    Task<CustomerDetails?> GetCustomerFromRepo(string id);
    Task<CustomerDetails?> GetCustomerFromRelational(string id);
}

public class CustomerService : ICustomerService
{
    private readonly ILogger _logger;
    private readonly IReadModelQuery _query;
    private readonly IAggregateRepository<Aggregates.Customer.Customer> _repo;
    
    public CustomerService(IAggregateRepository<Aggregates.Customer.Customer> aggregateRepo, IReadModelQuery query, ILogger<CustomerService> logger)
    {
        _repo = aggregateRepo;
        _logger = logger;
        _query = query;
    }

    public async Task<IEnumerable<CustomerDetails>?> GetAllCustomers()
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(GetAllCustomers)));

        try
        {
            return await _query.GetItemsAsync<CustomerDetails>(string.Empty);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get any customer records. Reason: {reason}", e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails?> GetCustomer(string id)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(GetCustomer)),
            new PropertyEnricher("AggregateId", id));

        try
        {
            return await _query.GetItemAsync<CustomerDetails>(id);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get a customer record. {id} Reason: {reason}", id, e.Message);
            throw;
        }
    }
    public async Task<CustomerDetails?> GetCustomerFromRepo(string id)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(GetCustomer)),
            new PropertyEnricher("AggregateId", id));

        try
        {
            var customer = await _repo.GetAsync(id);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get a customer record. {id} Reason: {reason}", id, e.Message);
            throw;
        }
    }
    public async Task<CustomerDetails?> GetCustomerFromRelational(string id)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(GetCustomer)),
            new PropertyEnricher("AggregateId", id));

        try
        {
            IDbConnection connection = new SqlConnection("Server=127.0.0.1;Database=CustomerApiStaging;User=sa;Password=P@ssw0rd;");
            var customer = await connection.QuerySingleAsync<CustomerDetailsEntity>(
                "SELECT TOP(1) * from CustomerDetails where AggregateId = @AggregateId", new { AggregateId = id });
            var addresses = await connection.QueryAsync<Address>("SELECT * from Addresses where ParentId = @ParentId",
                new { ParentId = customer.AggregateId });
            var phones = await connection.QueryAsync<Phone>("SELECT * from Phones where ParentId = @ParentId",
                new { ParentId = customer.AggregateId });
            customer.Addresses = addresses.ToList();
            customer.PhoneRecords = phones.ToList();

            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to get a customer record. {id} Reason: {reason}", id, e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails?> UpdateBasics(string id, BasicDetailsRequestParams basicDetailsRequestParams, Instant updatedOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(UpdateBasics)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.UpdateBasicDetails(basicDetailsRequestParams, updatedOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to update basic information on a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails?> AddAddress(string id, AddressDetailsRequestParams addressDetailsRequestParams, Instant addedOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(AddAddress)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.AddAddress(addressDetailsRequestParams, addedOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add an Address to a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails?> AddPhoneNumber(string id, PhoneDetailsRequestParams phoneDetailsRequestParams, Instant addedOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(AddPhoneNumber)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.AddPhoneRecord(phoneDetailsRequestParams, addedOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add an Address to a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails?> UpdateAddress(string id, Guid addressId, AddressDetailsRequestParams addressDetailsRequestParams, Instant updatedOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(UpdateAddress)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.UpdateAddress(addressId, addressDetailsRequestParams, updatedOn.ToDateTimeUtc(), "CorrelationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add an Address to a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails?> MakeAddressPrimaryShipping(string id, Guid addressId, Instant madePrimaryOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(AddPhoneNumber)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.MakeAddressPrimaryShipping(addressId, madePrimaryOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add an Address to a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails?> MakeAddressPrimaryBilling(string id, Guid addressId, Instant madeBillingOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(AddPhoneNumber)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.MakeAddressPrimaryBilling(addressId, madeBillingOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add an Address to a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails?> UpdatePhone(string id, Guid phoneId, PhoneDetailsRequestParams phoneDetailsRequestParams, Instant updatedOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(AddPhoneNumber)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.UpdatePhoneRecord(phoneId, phoneDetailsRequestParams, updatedOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add an Address to a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails?> MakePhonePrimary(string id, Guid phoneId, Instant madePrimaryOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(AddPhoneNumber)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.MakePhonePrimary(phoneId, madePrimaryOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add an Address to a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }

    

    public async Task<CustomerDetails> CreateCustomer(Guid newCustomerId, Instant createdOn, BasicDetailsRequestParams basicDetailsRequestParams)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(CreateCustomer)));

        try
        {
            var customer = new Aggregates.Customer.Customer(newCustomerId.ToString(), createdOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            customer.UpdateBasicDetails(basicDetailsRequestParams, createdOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, -1);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> RemoveCustomer(string id, Instant removedOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(AddPhoneNumber)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.RemoveCustomer(removedOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add an Address to a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails> RemoveAddress(string id, Guid addressId, Instant removedOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(AddPhoneNumber)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.RemoveAddress(addressId, removedOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add an Address to a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }

    public async Task<CustomerDetails> RemovePhone(string id, Guid phoneId, Instant removedOn)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerService)),
            new PropertyEnricher("Operation", nameof(AddPhoneNumber)));

        try
        {
            var customer = await _repo.GetAsync(id);
            customer.RemovePhoneRecord(phoneId, removedOn.ToDateTimeUtc(), "correlationId", Guid.Empty, "Davew");
            await _repo.SaveAsync(customer, customer.Version);
            return customer.ToDetails();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to add an Address to a customer record. {customerId} {reason}", id, e.Message);
            throw;
        }
    }
}