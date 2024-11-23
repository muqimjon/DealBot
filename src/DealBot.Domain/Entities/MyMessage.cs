namespace DealBot.Domain.Entities;

using DealBot.Domain.Common;
using DealBot.Domain.Enums;

public class MyMessage : Auditable
{
    public string Content { get; set; }
    public CardTypes CardType { get; set; } = CardTypes.None;
    public Genders Gender { get; set; } = Genders.Unknown;
    public Roles Role { get; set; } = Roles.None;
    public Status Status { get; set; } = Status.None;

    public User Sender { get; set; }
    public long SenderId { get; set; }

    public User Recipient { get; set; }
    public long? RecipientId { get; set; }
}