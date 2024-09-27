namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public class Address : Auditable
{
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string Longitude { get; set; }
    public string Latitude { get; set; }
}