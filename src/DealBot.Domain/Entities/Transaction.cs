namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;
using DealBot.Domain.Enums;

public sealed class Transaction : Auditable
{
    public decimal Amount { get; set; }
    public CashBackStatus Status { get; set; }
    public bool IsCashback { get; set; }

    public long SellerId { get; set; }
    public User Seller { get; set; }

    public long CustomerId { get; set; }
    public User Customer { get; set; }
}