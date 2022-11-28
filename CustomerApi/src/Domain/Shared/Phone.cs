using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper.Contrib.Extensions;
using PRDC2022.Customer.Domain.Projections;

namespace PRDC2022.Customer.Domain.Shared;

public class Phone
{
    [ExplicitKey]
    [System.ComponentModel.DataAnnotations.Key]
    public Guid PhoneId { get; set; }
    public bool IsPrimary { get; set; }
    public long Number { get; set; }
    public int CountryCode { get; set; }
    public string PhoneType { get; set; } = string.Empty;
    [ForeignKey("Parent")]
    [JsonIgnore]
    public string ParentId { get; set; }
    [Write(false)]
    [JsonIgnore]
    public CustomerDetailsEntity Parent { get; set; }


    public bool Equivalent(PhoneDetailsRequestParams parms)
    {
        return Number == parms.Number &&
               CountryCode == parms.CountryCode;
    }
}