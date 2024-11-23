﻿namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public class Address : Auditable
{
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}