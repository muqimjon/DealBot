namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;
using DealBot.Domain.Enums;

public sealed class User : Auditable
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset DateOfBirth { get; set; }
    public Genders Gender { get; set; }
    public decimal Balance { get; set; }
    public Roles Role { get; set; }
    public int TelegramId { get; set; }
    public TelegramStates State { get; set; }
    public bool IsActive { get; set; }

    public long ContactId { get; set; }
    public Contact Contact { get; set; } = default!;


    public long AddressId { get; set; }
    public Address Address { get; set; } = default!;

    public long AssetId { get; set; }
    public Asset Image { get; set; } = default!;

    public Referral? ReferredBy { get; set; }
    public Store Store { get; set; } = default!;
    public ICollection<Transaction> Transactions { get; set; } = default!;
    public ICollection<StoreReview> Reviews { get; set; } = default!;
    public ICollection<Referral> ReferralsInitiated { get; set; } = default!;
}