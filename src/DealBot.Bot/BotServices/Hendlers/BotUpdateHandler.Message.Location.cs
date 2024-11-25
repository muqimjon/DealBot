namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Entities;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task HandleLocationMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var handler = user.State switch
        {
            States.WaitingForSendLocation => HandleCompanyLocationAsync(botClient, message, cancellationToken),
            _ => HandleUnknownMessageAsync(botClient, message, cancellationToken)
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling message from {FirstName}", user.FirstName); }
    }

    private async Task SendRequestForLocationAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ReplyKeyboardMarkup keyboard = new([
            [new(localizer[Text.SendLocation]) { RequestLocation = true }],
            [new(localizer[Text.Back])]
        ])
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskForLocationInPlaceHolder],
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskLocation],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            messageId: message.MessageId,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendLocation;
    }

    private async Task HandleCompanyLocationAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var latitude = message.Location!.Latitude;
        var longitude = message.Location.Longitude;
        var address = await GetAddressFromCoordinatesAsync(longitude, latitude);
        await appDbContext.Addresses.AddAsync(address, cancellationToken);
        store.Address = address;
        await SendMenuAddressInfoAsync(botClient, message, cancellationToken, address: address);
    }

    //#nullable disable
    private static async Task<Address> GetAddressFromCoordinatesAsync(double longitude, double latitude)
    {
        using HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "DealBot/1.0");

        var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}&zoom=18&addressdetails=1";

        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            NominatimResponse nominatimResponse = JsonSerializer.Deserialize<NominatimResponse>(content,
                options: new() { PropertyNameCaseInsensitive = true })!;

            if (nominatimResponse.Address is null)
                return new()
                {
                    Latitude = latitude,
                    Longitude = longitude
                };
            else
                return new()
                {
                    Country = nominatimResponse.Address.Country,
                    CountryCode = nominatimResponse.Address.CountryCode,
                    Region = nominatimResponse.Address.State,
                    District = nominatimResponse.Address.County,
                    City = nominatimResponse.Address.Town,
                    Street = nominatimResponse.Address.Road,
                    HouseNumber = nominatimResponse.Address.HouseNumber,
                    Longitude = longitude,
                    Latitude = latitude
                };
        }
        catch { }

        return new()
        {
            Latitude = latitude,
            Longitude = longitude
        };
    }
}

#region Model
public class NominatimResponse
{
    public string DisplayName { get; set; } = string.Empty;
    public NominatimAddress Address { get; set; } = default!;
}

public class NominatimAddress
{
    public string Country { get; set; } = string.Empty;
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string Road { get; set; } = string.Empty;
    public string HouseNumber { get; set; } = string.Empty;
}
#endregion