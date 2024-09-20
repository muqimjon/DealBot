namespace DealBot.Application.Users.Commands.CreateUser;

using DealBot.Domain.Entities;
using DealBot.Domain.Enums;

public class UserResultDto
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public Genders Gender { get; set; }
    public decimal Balance { get; set; }
    public Roles Role { get; set; }
    public string LanguageCode { get; set; }
    public long TelegramId { get; set; }
    public bool IsPremium { get; set; }
    public bool IsBot { get; set; }
    public long ChatId { get; set; }
    public TelegramStates State { get; set; }
    public bool IsActive { get; set; }

    public long ContactId { get; set; }
    public Contact Contact { get; set; }


    public long AddressId { get; set; }
    public Address Address { get; set; }

    public long AssetId { get; set; }
    public Asset Image { get; set; }

    public Referral? ReferredBy { get; set; }
    public Store Store { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
    public ICollection<StoreReview> Reviews { get; set; }
    public ICollection<Referral> ReferralsInitiated { get; set; }
}