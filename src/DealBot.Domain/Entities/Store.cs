namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public sealed class Store : Auditable
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string MiniAppUrl { get; set; }
    public string Website { get; set; }
    public string Channel { get; set; }

    public Contact Contact { get; set; }
    public long? ContactId { get; set; }

    public Address Address { get; set; }
    public long? AddressId { get; set; }

    public Asset Image { get; set; }
    public long? AssetId { get; set; }

    public ICollection<StoreReview> Reviews { get; set; }
}