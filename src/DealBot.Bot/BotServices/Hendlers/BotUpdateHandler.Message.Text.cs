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

        if (message.Text == localizer[Text.Back])
        {
            await NavigateToPreviousPageAsync(botClient, message, cancellationToken);
            return;
        }

        var userState = message.Text == "/start" && user.State != States.None ? States.Restart : user.State;

        var handler = userState switch
        {
            States.None => SendFirstMenuLanguagesAsync(botClient, message, cancellationToken),
            States.Restart => user.Role switch
            {
                Roles.Admin => SendAdminMenuAsync(botClient, message, cancellationToken),
                Roles.Seller => SendSellerMenuAsync(botClient, message, cancellationToken),
                _ => SendCustomerMenuAsync(botClient, message, cancellationToken),
            },
            States.WaitingForSendComment => HandleCommentMessageAsync(botClient, message, cancellationToken),
            States.WaitingForSendEmail => HandleEmailAsync(botClient, message, cancellationToken),
            States.WaitingForSendFirstName => HandleFirstNameAsync(botClient, message, cancellationToken),
            States.WaitingForSendLastName => HandleLastNameAsync(botClient, message, cancellationToken),
            States.WaitingForSendName => HandleNameAsync(botClient, message, cancellationToken),
            States.WaitingForSendUserId => HandleUserIdAsync(botClient, message, cancellationToken),
            States.WaitingForSendProductPrice => HandleProductPriceAsync(botClient, message, cancellationToken),
            States.WaitingForSendSalesAmount => HandleSalesAmountAsync(botClient, message, cancellationToken),
            States.WaitingForSendMessageToDeveloper => HandleMessageToDeveloperAsync(botClient, message, cancellationToken),
            States.WaitingForSendDescription => HandleDesctiptionAsync(botClient, message, cancellationToken),
            States.WaitingForSendMiniAppUrl => HandleMiniAppUrlAsync(botClient, message, cancellationToken),
            States.WaitingForSendWebsite => HandleWebsiteAsync(botClient, message, cancellationToken),
            States.WaitingForSendCompanyEmail => HandleCompanyEmailAsync(botClient, message, cancellationToken),
            States.WaitingForSendChannel => HandleChannelAsync(botClient, message, cancellationToken),
            States.WaitingForSendMessageText => HandleMessageTextAsync(botClient, message, cancellationToken),
            _ => HandleUnknownMessageAsync(botClient, message, cancellationToken)
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling message from {user.FirstName}", user.FirstName); }
    }
}
