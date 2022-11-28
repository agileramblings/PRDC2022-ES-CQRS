using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;

namespace PRDC2022.CustomerApi.Persistence
{
    [System.ComponentModel.DataAnnotations.Schema.Table("CustomerAddressAnalytics")]
    public class CustomerAddressAnalyticsEntity
    {
        [System.ComponentModel.DataAnnotations.Key]
        [ExplicitKey]
        [StringLength(100)]
        public string CustomerId { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string MiddleName { get; set; }

        [StringLength(50)] 
        public string LastName { get; set; }
        
        public int NumberOfAddresses { get; set; }
        public int NumberOfPhoneNumbers { get; set; }
    }
}