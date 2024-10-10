namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

public partial class BotUpdateHandler
{
    private async Task HandleTextMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message is null || message.Text is null)
            return;

        if (message.Text.Equals(localizer[Text.Back]))
        {
            await NavigateToPreviousPageAsync(botClient, message, cancellationToken);
            return;
        }

        var userState = message.Text.Equals("/start") && !user.State.Equals(States.None) ? States.Restart : user.State;

        var handler = userState switch
        {
            States.None => SendFirstMenuLanguagesAsync(botClient, message, cancellationToken),
            States.Restart => user.Role switch
            {
                Roles.Customer => SendCustomerMenuAsync(botClient, message, cancellationToken),
                Roles.Seller => SendSellerMenuAsync(botClient, message, cancellationToken),
                _ => default!
            },
            States.WaitingForSendComment => HandleCommentMessageAsync(botClient, message, cancellationToken),
            States.WaitingForSendEmail => HandleEmailAsync(botClient, message, cancellationToken),
            States.WaitingForSendFirstName => HandleFirstNameAsync(botClient, message, cancellationToken),
            States.WaitingForSendLastName => HandleLastNameAsync(botClient, message, cancellationToken),
            States.WaitingForSendName => HandleNameAsync(botClient, message, cancellationToken),
            States.WaitingForSendAbout => HandleAboutAsync(botClient, message, cancellationToken),
            _ => HandleUnknownMessageAsync(botClient, message, cancellationToken)
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling message from {user.FirstName}", user.FirstName); }
    }
}
