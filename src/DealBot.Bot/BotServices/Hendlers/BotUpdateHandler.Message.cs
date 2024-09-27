namespace DealBot.Bot.BotServices;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public partial class BotUpdateHandler
{
    private async Task HandleMessageAsync(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var handler = message.Type switch
        {
            MessageType.Text => HandleTextMessageAsync(botClient, message, cancellationToken),
            MessageType.Contact => HandleContactMessageAsync(botClient, message, cancellationToken),
            _ => HandleUnknownMessageAsync(botClient, message, cancellationToken),
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling message from {user.FirstName}", user); }
    }

    private Task HandleUnknownMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken _)
    {
        logger.LogInformation("Received message type {message.Type} from {message.From.FirstName}", message, message);
        return Task.CompletedTask;
    }
}
