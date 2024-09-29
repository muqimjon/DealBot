namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;
using DealBot.Domain.Enums;

public class Card : Auditable
{
    public decimal Ballance { get; set; }
    public CardStus Type { get; set; }
    public bool IsActive { get; set; }
}