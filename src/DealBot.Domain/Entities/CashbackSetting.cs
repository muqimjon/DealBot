namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;
using DealBot.Domain.Enums;

public class CashbackSetting : Auditable
{
    public CardType Type { get; set; }
    public decimal Percentage { get; set; }
}