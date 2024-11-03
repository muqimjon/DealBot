namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task HandleContactMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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


    private async Task SendRequestPhoneNumberAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, Domain.Entities.User value = default!)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        value ??= user;

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.SendContact]) { RequestContact = true }]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskPhoneNumberInPlaceHolder],
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskPhoneNumber, value.Contact.Phone!],
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

        var value = user;
        if (user.PlaceId != 0 && (value = await appDbContext.Users
            .Include(u => u.Contact)
            .FirstOrDefaultAsync(u => u.Id == user.PlaceId, cancellationToken)) is null)
        {
            await SendAdminMenuAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        if (user.Contact is null)
            await appDbContext.Contacts.AddAsync(user.Contact = new(), cancellationToken);


        var actionMessage = localizer[string.IsNullOrEmpty(value.Contact.Phone) ? Text.SetSucceeded : Text.UpdateSucceeded];
        value.Contact.Phone = message.Contact.PhoneNumber;

        await SendCustomerMenuAsync(botClient, message, cancellationToken, actionMessage);
    }


    private async Task SendRequestCompanyPhoneNumberAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.SendContact]){ RequestContact = true}],
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskPhoneNumberInPlaceHolder],
        };

        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .Include(s => s.Contact)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        store.Contact ??= new();
        var phone = string.IsNullOrEmpty(store.Contact.Phone) ? Text.Undefined : store.Contact.Phone;

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskPhoneNumber, phone],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendCompanyPhoneNumber;
    }

    // TO DO need Validation
    private async Task HandleCompanyPhoneNumberAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Contact);

        var store = await appDbContext.Stores
            .Include(s => s.Contact)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var actionMessage = localizer[string.IsNullOrEmpty(store.Contact.Phone) ? Text.SetSucceeded : Text.UpdateSucceeded];
        store.Contact.Phone = message.Contact.PhoneNumber;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, actionMessage);
    }
}
