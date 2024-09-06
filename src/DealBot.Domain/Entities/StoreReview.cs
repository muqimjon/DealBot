namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public class StoreReview : Auditable
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    public long StoreId { get; set; }
    public Store Store { get; set; } = default!;

    public long UserId { get; set; }
    public User User { get; set; } = default!;
}