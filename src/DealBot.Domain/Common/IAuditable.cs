namespace DealBot.Domain.Common;

public interface IAuditable
{
    long Id { get; set; }
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
}