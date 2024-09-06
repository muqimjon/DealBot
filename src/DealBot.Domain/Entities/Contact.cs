namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public sealed class Contact : Auditable
{
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}