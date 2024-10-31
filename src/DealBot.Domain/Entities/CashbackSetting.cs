namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;
using DealBot.Domain.Enums;

public class CashbackSetting : Auditable
{
    public CardTypes Type { get; set; }
    public decimal Percentage { get; set; }
    public bool IsActive { get; set; }
}