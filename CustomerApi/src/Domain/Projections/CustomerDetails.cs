using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Contrib.Extensions;
using DepthConsulting.Core.DDD;
using PRDC2022.Customer.Domain.Shared;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace PRDC2022.Customer.Domain.Projections;

public class CustomerDetails : ProjectionBase
{
    public CustomerDetails() : base("tbd")
    {
        // DO NOT USE - For Serializers only
    }
    public CustomerDetails(string aggregateId) : base(aggregateId)
    {
    }

    public string Type => nameof(CustomerDetails);
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime DateOfBirth { get; set; } = new(1900,1,1);
    public string Email { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public List<Address> Addresses { get; set; } = new();
    public List<Phone> PhoneRecords { get; set;  } = new();
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime LastModifiedOn { get; set; }

    public CustomerDetailsEntity ToEntity()
    {
        return new CustomerDetailsEntity
        {
            AggregateId = this.AggregateId,
            Version = this.Version,
            Addresses = Addresses,
            PhoneRecords = PhoneRecords,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            DateOfBirth = DateOfBirth,
            Email = Email,
            ModifiedBy = ModifiedBy,
            LastModifiedOn = LastModifiedOn,
            CreatedBy = CreatedBy,
            CreatedOn = CreatedOn
        };
    }
}

[Table("CustomerDetails")]
public class CustomerDetailsEntity
{
    [ExplicitKey]
    [System.ComponentModel.DataAnnotations.Key]
    public string AggregateId { get; set; }
    public int Version { get; set; }
    [Computed]
    public string Type => nameof(CustomerDetailsEntity);
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime DateOfBirth { get; set; } = new(1900,1,1);
    public string Email { get; set; } = string.Empty;
    public string? Gender { get; set; }
    [NotMapped]
    [Write(false)]
    public List<Address> Addresses { get; set; } = new();
    [NotMapped]
    [Write(false)]
    public List<Phone> PhoneRecords { get; set;  } = new();
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime LastModifiedOn { get; set; }

    public CustomerDetails ToDetails()
    {
        return new CustomerDetails(AggregateId)
        {
            Version = Version,
            CreatedBy = CreatedBy,
            CreatedOn = CreatedOn,
            ModifiedBy = ModifiedBy,
            LastModifiedOn = LastModifiedOn,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Gender = Gender,
            DateOfBirth = DateOfBirth,
            Email = Email,
            PhoneRecords = PhoneRecords,
            Addresses = Addresses
        };
    }
}

