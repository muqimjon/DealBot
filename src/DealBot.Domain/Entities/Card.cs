namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;
using DealBot.Domain.Enums;

public class Card : Auditable
{
    public decimal Ballance { get; set; }
    public CardType Type { get; set; }
    public CardState State { get; set; }
}