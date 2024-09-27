namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public sealed class Contact : Auditable
{
    public string Phone { get; set; }
    public string? Email { get; set; }
}