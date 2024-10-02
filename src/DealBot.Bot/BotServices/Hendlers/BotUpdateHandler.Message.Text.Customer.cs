namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendRequestCommentAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskCommentInPlaceHolder],
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskComment],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendComment;
    }

    private async Task HandleCommentMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.DeleteMessageAsync(
                chatId: message.Chat.Id,
                messageId: user.MessageId,
                cancellationToken: cancellationToken);
        }
        catch { }

        await SendCustomerMenuAsync(botClient, message, cancellationToken, localizer[Text.ReceiveComment]);
    }

    private async Task SendRequestFirstNameAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskFirstNameInPlaceHolder],
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskFirstName, user.FirstName],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendFirstName;
    }

    private async Task HandleFirstNameAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        // TO DO Need validation
        user.FirstName = message.Text!;
        await SendMenuChangeCustomerInfoAsync(botClient, message, cancellationToken);
    }

    private async Task SendRequestLastNameAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskLastNameInPlaceHolder],
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskLastName, user.LastName],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendLastName;
    }

    private async Task HandleLastNameAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        // TO DO Need validation
        user.LastName = message.Text!;
        await SendMenuChangeCustomerInfoAsync(botClient, message, cancellationToken);
    }

    private async Task SendRequestEmailAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskEmailInPlaceHolder],
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskEmail, user.Contact.Email!],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendEmail;
    }

    private async Task HandleEmailAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        // TO DO Need validation
        user.Contact.Email = message.Text;
        await SendMenuChangeCustomerInfoAsync(botClient, message, cancellationToken);
    }
}
