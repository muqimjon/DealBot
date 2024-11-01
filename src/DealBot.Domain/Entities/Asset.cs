namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public sealed class Asset : Auditable
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string? FileId { get; set; }
}