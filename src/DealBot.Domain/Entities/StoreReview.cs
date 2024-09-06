namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public class StoreReview : Auditable
{
    public int Rating { get; set; }
    public string Comment { get; set; }

    public long StoreId { get; set; }
    public Store Store { get; set; }

    public long UserId { get; set; }
    public User User { get; set; }
}