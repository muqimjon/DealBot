namespace DealBot.Domain.Common;

public abstract class Auditable : IAuditable
{
    public long Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
