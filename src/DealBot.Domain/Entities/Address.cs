namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public class Address : Auditable
{
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
}