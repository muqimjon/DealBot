namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task HandlePhotoMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received Photo message from {FirstName}", user.FirstName);

        if (message is null || message.Photo is null)
            return;

        var handler = user.State switch
        {
            States.WaitingForSendCompanyImage => HandleBotPicAsync(botClient, message, cancellationToken),
            _ => HandleUnknownMessageAsync(botClient, message, cancellationToken)
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling message from {user.FirstName}", user.FirstName); }
    }

    private async Task SendRequestForPictureAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.UploadPhoto,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .Include(s => s.Image)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        if (store.Image is null)
            await appDbContext.Assets.AddAsync(store.Image = new()
            {
                FilePath = "https://b1547017.smushcdn.com/1547017/wp-content/uploads/2022/05/seo-16x9-1.jpg?lossy=1&strip=1&webp=1",
                FileName = "StoreImage",
            }, cancellationToken);

        Stream stream;
        bool isExist;
        try
        {
            var file = await botClient.GetFileAsync(store.Image.FileId!, cancellationToken);
            stream = new MemoryStream();
            await botClient.DownloadFileAsync(file.FilePath!, stream, cancellationToken);
            stream.Position = 0;
            isExist = true;
        }
        catch
        {
            using HttpClient httpClient = new();
            var imageBytes = await httpClient.GetByteArrayAsync(store.Image.FilePath, cancellationToken);
            stream = new MemoryStream(imageBytes);
            isExist = false;
        }

        InlineKeyboardMarkup keyboard = new(
        [
            isExist
                ? [InlineKeyboardButton.WithCallbackData(localizer[Text.Delete], CallbackData.Delete)]
                : Array.Empty<InlineKeyboardButton>(),
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        ]);

        await using (stream)
        {
            var sentMessage = await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: InputFile.FromStream(stream),
                caption: localizer[Text.AskPhoto],
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

            user.MessageId = sentMessage.MessageId;
            user.State = States.WaitingForSendCompanyImage;
        }
    }

    private async Task HandleBotPicAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Photo, nameof(message));

        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .Include(s => s.Image)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        store.Image.FileId = message.Photo.Last().FileId;
        var actionMessage = localizer[string.IsNullOrEmpty(store.Image.FileId) ? Text.SetSucceeded : Text.UpdateSucceeded];
        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, actionMessage);
    }

    private async Task HandleSelectedCompanyImageAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(callbackQuery));

        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .Include(s => s.Image)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.Error]);
            return;
        }

        store.Image.FileId = Text.Empty;
        await SendMenuCompanyInfoAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.DeleteSucceeded]);
    }
}
