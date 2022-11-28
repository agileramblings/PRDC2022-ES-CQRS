using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper.Contrib.Extensions;
using PRDC2022.Customer.Domain.Projections;

namespace PRDC2022.Customer.Domain.Shared;

[Dapper.Contrib.Extensions.Table("Addresses")]
public class Address
{
    [ExplicitKey]
    [System.ComponentModel.DataAnnotations.Key]
    public Guid AddressId { get; set; }

    public bool IsPrimaryShipping { get; set; }
    public bool IsPrimaryBilling { get; set; }
    public string Address1 { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    [ForeignKey("Parent")] 
    [JsonIgnore] 
    public string ParentId { get; set; }

    [Write(false)] 
    [JsonIgnore] 
    public CustomerDetailsEntity Parent { get; set; }


    public bool Equivalent(AddressDetailsRequestParams parms)
    {
        return Address1 == parms.Address1 &&
               Address2 == parms.Address2 &&
               City == parms.City &&
               Region == parms.Region &&
               Country == parms.Country &&
               PostalCode == parms.PostalCode;
    }
}