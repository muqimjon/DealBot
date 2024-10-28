namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;
using DealBot.Domain.Enums;

public class Card : Auditable
{
    public decimal Ballance { get; set; }
    public CardTypes Type { get; set; }
    public CardStates State { get; set; }
}