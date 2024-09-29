namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;
using DealBot.Domain.Enums;

public sealed class User : Auditable
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public Genders Gender { get; set; }
    public Roles Role { get; set; }
    public string LanguageCode { get; set; }
    public long TelegramId { get; set; }
    public States State { get; set; }
    public int MessageId { get; set; }
    public bool IsActive { get; set; }

    public Card Card { get; set; }
    public long? CardId { get; set; }

    public Contact Contact { get; set; }
    public long? ContactId { get; set; }


    public Address Address { get; set; }
    public long? AddressId { get; set; }

    public Asset Image { get; set; }
    public long? AssetId { get; set; }

    public Referral? ReferredBy { get; set; }
    public Store Store { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
    public ICollection<StoreReview> Reviews { get; set; }
    public ICollection<Referral> ReferralsInitiated { get; set; }
}