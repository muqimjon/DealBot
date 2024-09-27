namespace DealBot.Bot.BotServices;

using DealBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

public partial class BotUpdateHandler
{
    private async Task HandleTextMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message is null || message.Text is null)
            return;

        var userState = message.Text.Equals("/start") && !user.State.Equals(States.None) ? States.Restart : user.State;

        var handler = userState switch
        {
            States.None => SendLanguagesMenuAsync(botClient, message, cancellationToken),
            States.Restart => user.Role switch
                {
                    Roles.Seller => SendSellerMenuAsync(botClient, message, cancellationToken),
                    Roles.Customer => SendCustomerMenuAsync(botClient, message, cancellationToken),
                    _ => SendGreetingAsync(botClient, message, cancellationToken),
                },
            _ => HandleUnknownMessageAsync(botClient, message, cancellationToken)
        } ;

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling message from {user.FirstName}", user.FirstName); }
    }

    private async Task SendSellerMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        
    }
}
