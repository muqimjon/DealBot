namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task HandleLocationMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received Contact message query from {FirstName}", user.FirstName);

        var handler = user.State switch
        {
            States.WaitingForSendPhoneNumber => HandleSentPhoneNumberAsync(botClient, message, cancellationToken),
            States.WaitingForSendCompanyPhoneNumber => HandleCompanyPhoneNumberAsync(botClient, message, cancellationToken),
            _ => HandleUnknownMessageAsync(botClient, message, cancellationToken)
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling message from {FirstName}", user.FirstName); }
    }

    private async Task SendRequestForLocationAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.SendLocation]) { RequestLocation = true }],
            [new(localizer[Text.Back])]
        })
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

}
