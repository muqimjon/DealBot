namespace DealBot.Bot.BotServices;

using Telegram.Bot.Types;
using Telegram.Bot;
using DealBot.Domain.Enums;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using DealBot.Bot.Resources;

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

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.ReceiveComment],
            cancellationToken: cancellationToken);

        await SendCustomerMenuAsync(botClient, message, cancellationToken);
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
            text: localizer[Text.AskEmail],
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
            text: localizer[Text.AskFirstName],
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
            text: localizer[Text.AskLastName],
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
}
