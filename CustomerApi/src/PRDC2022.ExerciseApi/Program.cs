// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices.ComTypes;
using GenFu;
using PRDC2022.ExerciseApi.GeneratedClient;
Console.WriteLine("Running client to exercise customer api...");

var c = new Client("https://localhost:7260/", new HttpClient());
var rnd = new Random();
GenFu.GenFu.Configure<PhoneDetailsRequestParams>()
    .Fill(p => p.Number, () => rnd.Next(100000000, 600000000))
    .Fill(p => p.CountryCode, () => rnd.Next(1,9));

var customerIds = new List<string>();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
Task.Run(() =>
{
    var rnd = new Random();
    while (true)
    {
        if (customerIds.Any())
        {
            var index = rnd.Next(0, customerIds.Count);
            var customerId = customerIds[index];
            _ = c.CustomerGETAsync(customerId).GetAwaiter().GetResult();
        }
        else
        {
            Task.Delay(1000).GetAwaiter().GetResult();
        }
        Task.Delay(100).GetAwaiter().GetResult();
    }
});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

while (true)
{
    var customerCreation = A.New<BasicDetailsRequestParams>();
    var customer = await c.CustomerPOSTAsync(customerCreation);
    customerIds.Add(customer.AggregateId);
    var numOfAddresses = rnd.Next(2, 4);
    for (var i = 0; i < numOfAddresses; i++)
    {
        var address = A.New<AddressDetailsRequestParams>();
        customer = await c.AddressPOSTAsync(customer.AggregateId, address);
        if (i == 0)
        {
            customer = await c.PrimarybillingAsync(customer.AggregateId,
                customer.Addresses.Single(a => a.Address1 == address.Address1).AddressId);
        }
        else if(i == 1)
        {
            customer = await c.PrimaryshippingAsync(customer.AggregateId,
                customer.Addresses.Single(a => a.Address1 == address.Address1).AddressId);
        }

        await Task.Delay(1000);
    }

    var numberOfPhones = rnd.Next(3, 50);
    for (var j = 0; j < numberOfPhones; j++)
    {
        var phone = A.New<PhoneDetailsRequestParams>();
        customer = await c.PhonePOSTAsync(customer.AggregateId, phone);
        if (j == 0)
        {
            customer = await c.PrimaryAsync(customer.AggregateId,
                customer.PhoneRecords.Single(pr => pr.Number == phone.Number).PhoneId);
        }

        await Task.Delay(1000);
    }

    await Task.Delay(1000);
}
