namespace PRDC2022.Customer.Domain.Shared;

public class BasicDetailsRequestParams
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Gender { get; set; }
}

public class AddressDetailsRequestParams
{
    public string Address1 { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

public class PhoneDetailsRequestParams
{
    public long Number { get; set; }
    public int CountryCode { get; set; }
    public string PhoneType { get; set; } = string.Empty;
}