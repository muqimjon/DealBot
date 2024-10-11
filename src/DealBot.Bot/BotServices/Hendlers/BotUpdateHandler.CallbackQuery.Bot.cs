﻿namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendBotSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Picture], CallbackData.Picture),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Name], CallbackData.Name)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Description], CallbackData.Description),
                InlineKeyboardButton.WithCallbackData(localizer[Text.About], CallbackData.About)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.DescriptionPicture], CallbackData.DescriptionPicture),
                InlineKeyboardButton.WithCallbackData(localizer[Text.MiniAppUrl], CallbackData.MiniAppUrl)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });
        Message sentMessage = default!;
        var text = string.Concat(actionMessage, localizer[Text.SelectSettings]);

        try
        {
            sentMessage = await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
        catch
        {
            try
            {
                await botClient.SendChatActionAsync(
                    chatId: message.Chat.Id,
                    chatAction: ChatAction.Typing,
                    cancellationToken: cancellationToken);

                sentMessage = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: text,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);

                await botClient.DeleteMessageAsync(
                    messageId: user.MessageId,
                    chatId: message.Chat.Id,
                    cancellationToken: cancellationToken);
            }
            catch { }
            try
            {
                await botClient.DeleteMessageAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    cancellationToken: cancellationToken);
            }
            catch { }
        }

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectBotSettings;
    }

    private async Task HandleBotSettings(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.Picture => SendRequestForPictureAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Name => SendRequestForNameAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Description => SendRequestForDescriptionAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.About => SendRequestForAboutAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.DescriptionPicture => SendRequestForDescriptionPictureAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.MiniAppUrl => SendRequestForMiniAppUrlAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => default!,
        });
    }

    private async Task SendRequestForDescriptionPictureAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);
    }
}