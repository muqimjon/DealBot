﻿namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendSellerMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.CustomerManager], CallbackData.UserManager)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.CustomersList], CallbackData.CustomersList)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.SendMessage], CallbackData.SendMessage),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Statistics], CallbackData.Statistics)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Settings], CallbackData.Settings)],
        ]);

        var text = string.Concat(actionMessage, localizer[Text.SelectMenu]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.PlaceId = 0;
        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectMenu;
    }

    private async Task HandleSelectedSellerMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.UserManager => SendRequestForUserIdAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.CustomersList => SendCustomerListAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Statistics => SendStatisticsAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.SendMessage => HandleSendMessage(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Settings => SendSellerSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task SendSellerSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ChangeLanguage], CallbackData.ChangeLanguage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.PersonalInfo], CallbackData.PersonalInfo)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.MessageToDeveloper], CallbackData.SendMessage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

        var text = string.Concat(actionMessage, localizer[Text.SelectSettings]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectSettings;
    }

    private async Task HandleSelectedSellerSettingsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.ChangeLanguage => SendMenuLanguagesAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.PersonalInfo => SendMenuPersonalInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.CompanyInfo => SendMenuCompanyInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.SendMessage => SendRequestMessageToDeveloperAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }


    private async Task SendStatisticsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);
    }


    private async Task SendUserManagerMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Transaction], CallbackData.Transaction)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Statistics], CallbackData.Statistics)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Settings], CallbackData.Settings)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        ]);

        var text = string.Concat(actionMessage, localizer[Text.SelectMenu]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectUserMenu;
    }

    private async Task HandleSelectedUserMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.Transaction => CheckMissionTransactionAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Statistics => SendUserStatisticsAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Settings => user.Role switch
            {
                Roles.Admin => SendAdminUserSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
                Roles.Seller => SendSellerUserSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
                _ => default!
            },
            _ => default!
        });
    }


    private async Task SendUserStatisticsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);
    }


    private async Task SendCustomerListAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var customers = appDbContext.Users
            .Include(u => u.Contact)
            .Where(u => u.Role.Equals(Roles.Customer));

        StringBuilder text = new();

        foreach (var customer in customers)
            text.Append($"_{customer.GetFullName()}_ \\{customer.Contact.Phone}\n");

        var xabar = string.IsNullOrEmpty(text.ToString()) ? Text.NotAvailable : text.ToString();

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        ]);

        await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: xabar,
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = message.MessageId;
        user.State = States.CustomersList;
    }
}
