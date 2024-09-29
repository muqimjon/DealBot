namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task HandleContactMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        logger.LogInformation("Received Contact message query from {FirstName}", user.FirstName);

        var handler = user.State switch
        {
            States.WaitingForSendPhoneNumber => HandleSentPhoneNumberAsync(botClient, message, cancellationToken),
            _ => HandleUnknownMessageAsync(botClient, message, cancellationToken)
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling message from {FirstName}", user.FirstName); }
    }

    private async Task SendRequestForPhoneNumberAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.SendPhoneNumber]) { RequestContact = true }]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = "Telefon raqamingizni kiriting..",
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskPhoneNumber],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            messageId: message.MessageId,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendPhoneNumber;
    }

    private async Task HandleSentPhoneNumberAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Contact);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: user.MessageId,
            cancellationToken: cancellationToken);

        if (user.Contact is null)
        {
            user.Contact = new Domain.Entities.Contact
            {
                Phone = message.Contact.PhoneNumber
            };

            await appDbContext.SaveChangesAsync(cancellationToken);
            //user.ContactId = user.Contact.Id;
        }
        else
            user.Contact.Phone = message.Contact.PhoneNumber;

        await SendCustomerMenuAsync(botClient, message, cancellationToken);
    }
}
