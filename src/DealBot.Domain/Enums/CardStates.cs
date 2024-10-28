namespace DealBot.Domain.Enums;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CardStates
{
    Block,
    Active,
}