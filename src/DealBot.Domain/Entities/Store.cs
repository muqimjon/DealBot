namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public sealed class Store : Auditable
{
    public string Name { get; set; } = string.Empty;
    public decimal CashBackPersentage { get; set; }
    public bool IsActive { get; set; }

    public long ContactId { get; set; }
    public Contact Contact { get; set; } = default!;

    public long AddressId { get; set; }
    public Address Address { get; set; } = default!;

    public long OwnerId { get; set; }
    public User Owner { get; set; } = default!;

    public long AssetId { get; set; }
    public Asset Image { get; set; } = default!;

    public ICollection<Transaction> Transactions { get; set; } = default!;
    public ICollection<StoreReview> Reviews { get; set; } = default!;
}