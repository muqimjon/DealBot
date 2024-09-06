namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;

public sealed class Asset : Auditable
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}