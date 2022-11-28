using System.Diagnostics;
using DepthConsulting.Core.DDD.Exceptions;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using PRDC2022.Customer.Domain.Projections;
using PRDC2022.Customer.Domain.Services;
using PRDC2022.Customer.Domain.Shared;
using PRDC2022.CustomerApi.Persistence.Cosmos;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace PRDC2022.CustomerApi.Controllers;

[ApiController]
[Route("customer")]
public class CustomerController : ControllerBase
{
    private readonly ILogger<CustomerController> _logger;
    private readonly ICustomerService _customerService;
    private readonly IClock _clock;

    public CustomerController(ICustomerService customerService, IClock clock, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _clock = clock;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDetails>>> GetAll()
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(GetAll)));
        var sw = new Stopwatch();
        try
        {

            sw.Start();
            var customers = await _customerService.GetAllCustomers();
            sw.Stop();
            return Ok(customers);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound();
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDetails>> Get(string id)
    {
        var selectGetMethod = new Random().Next(0, 1000000) % 3;
        var getMethod = selectGetMethod == 1 ? "Cosmos" : selectGetMethod == 2 ? "EventStore" : "Relational";
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(Get)),
            new PropertyEnricher("AggregateId", id),
            new PropertyEnricher("GetMethod", getMethod));
        var sw = new Stopwatch();
        try
        {
            sw.Start();
            var customer = selectGetMethod switch
            {
                1 => await _customerService.GetCustomer(id),
                2 => await _customerService.GetCustomerFromRepo(id),
                _ => await _customerService.GetCustomerFromRelational(id)
            };
            sw.Stop();
            return Ok(customer);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDetails>> Create([FromBody]BasicDetailsRequestParams basicDetailsRequestParams)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(Create)));
        var sw = new Stopwatch();

        try
        {
            var newCustomerId = Guid.NewGuid();
            sw.Start();
            var newCustomer = await _customerService.CreateCustomer(newCustomerId, _clock.GetCurrentInstant(), basicDetailsRequestParams);
            sw.Stop();
            return Ok(newCustomer);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<CustomerDetails>> UpdateBasics(string id, [FromBody] BasicDetailsRequestParams basicDetailsRequestParams)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(UpdateBasics)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var customerRecord = await _customerService.UpdateBasics(id, basicDetailsRequestParams, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(customerRecord);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpPost("{id}/address")]
    public async Task<ActionResult<CustomerDetails>> AddAddress(string id, [FromBody] AddressDetailsRequestParams addressDetailsRequestParams)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(AddAddress)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var customerRecord = await _customerService.AddAddress(id, addressDetailsRequestParams, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(customerRecord);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpPost("{id}/phone")]
    public async Task<ActionResult<CustomerDetails>> AddPhone(string id, [FromBody] PhoneDetailsRequestParams phoneDetailsRequestParams)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(AddPhone)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var customerRecord = await _customerService.AddPhoneNumber(id, phoneDetailsRequestParams, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(customerRecord);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }
    
    [HttpPatch("{id}/address/{addressId}")]
    public async Task<ActionResult<CustomerDetails>> UpdateAddress(string id, Guid addressId, [FromBody] AddressDetailsRequestParams addressDetailsRequestParams)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(UpdateAddress)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var response = await _customerService.UpdateAddress(id, addressId, addressDetailsRequestParams, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(response);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpPatch("{id}/address/{addressId}/primaryshipping")]
    public async Task<ActionResult<CustomerDetails>> MakeAddressPrimaryShipping(string id, Guid addressId)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(MakeAddressPrimaryShipping)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var response = await _customerService.MakeAddressPrimaryShipping(id, addressId, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(response);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpPatch("{id}/address/{addressId}/primarybilling")]
    public async Task<ActionResult<CustomerDetails>> MakeAddressPrimaryBilling(string id, Guid addressId)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(MakeAddressPrimaryBilling)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var response = await _customerService.MakeAddressPrimaryBilling(id, addressId, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(response);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpPatch("{id}/phone/{phoneId}")]
    public async Task<ActionResult<CustomerDetails>> UpdatePhone(string id, Guid phoneId, [FromBody]PhoneDetailsRequestParams phoneDetailsRequestParams)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(UpdatePhone)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var response = await _customerService.UpdatePhone(id, phoneId, phoneDetailsRequestParams, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(response);

        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpPatch("{id}/phone/{phoneId}/primary")]
    public async Task<ActionResult<CustomerDetails>> MakePhonePrimary(string id, Guid phoneId)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(MakePhonePrimary)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var response = await _customerService.MakePhonePrimary(id, phoneId, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(response);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> RemoveCustomer(string id)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(RemoveCustomer)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var response = await _customerService.RemoveCustomer(id, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(response);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpDelete("{id}/address/{addressId}")]
    public async Task<ActionResult<CustomerDetails>> RemoveAddress(string id, Guid addressId)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(RemoveAddress)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var response = await _customerService.RemoveAddress(id, addressId, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(response);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }

    [HttpDelete("{id}/phone/{phoneId}")]
    public async Task<ActionResult<CustomerDetails>> RemovePhone(string id, Guid phoneId)
    {
        using var prop = LogContext.Push(
            new PropertyEnricher("Component", nameof(CustomerController)),
            new PropertyEnricher("Operation", nameof(RemovePhone)));
        var sw = new Stopwatch();

        try
        {
            sw.Start();
            var response = await _customerService.RemovePhone(id, phoneId, _clock.GetCurrentInstant());
            sw.Stop();
            return Ok(response);
        }
        catch (AggregateNotFoundException e)
        {
            return NotFound(id);
        }
        finally
        {
            _logger.LogInformation("Execution time: {elapsedMilliseconds}", sw.ElapsedMilliseconds);
        }
    }
}

