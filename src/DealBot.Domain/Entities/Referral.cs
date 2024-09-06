namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public sealed class Referral : Auditable
{
    public string ReferralCode { get; set; }
    public bool IsRewarded { get; set; }
    public decimal RewardAmount { get; set; }

    public long ReferrerId { get; set; }
    public User Referrer { get; set; }

    public long ReferredId { get; set; }
    public User Referred { get; set; }
}