namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task HandlePhotoMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message is null || message.Photo is null)
            return;

        var handler = user.State switch
        {
            States.WaitingForSendBotPic => HandleBotPicAsync(botClient, message, cancellationToken),
            _ => HandleUnknownMessageAsync(botClient, message, cancellationToken)
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling message from {user.FirstName}", user.FirstName); }
    }

    private async Task SendRequestForPictureAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
            InputFieldPlaceholder = localizer[Text.ActionNotAvailable],
        };

        var bot = await botClient.GetMeAsync(cancellationToken);

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskPhoto],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendBotPic;
    }

    private async Task HandleBotPicAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        //var photo = message.Photo?.LastOrDefault();
        //if (photo is not null)
        //{
        //    var file = await botClient.GetFileAsync(photo.FileId, cancellationToken);
        //    using var fileStream = new FileStream(file.FilePath!, FileMode.Open, FileAccess.Read);

        //    await botClient.SetChatPhotoAsync(
        //        chatId: message.Chat.Id,
        //        photo: new InputFileStream(fileStream),
        //        cancellationToken: cancellationToken);
        //}

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken);
    }

}
