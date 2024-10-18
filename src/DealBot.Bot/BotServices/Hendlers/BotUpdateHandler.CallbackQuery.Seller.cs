namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendSellerMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string messageText = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.UserManager], CallbackData.UserMamager)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.CustomersList], CallbackData.CustomersList)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.SendMessage], CallbackData.SendMessage),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Statistics], CallbackData.Statistics)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Settings], CallbackData.Settings)],
        });
        var text = string.Concat(messageText, localizer[Text.SelectMenu]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectMenu;
    }

    private async Task HandleSelectedSellerMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.UserMamager => SendRequestForUserIdAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.CustomersList => SendCustomerListAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Statistics => SendStatisticsAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.SendMessage => SendMessageMenuAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Settings => SendSellerSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task SendSellerSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ChangeLanguage], CallbackData.ChangeLanguage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.PersonalInfo], CallbackData.PersonalInfo)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.CompanyInfo], CallbackData.CompanyInfo)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.MessageToDeveloper], CallbackData.MessageToDeveloper)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var text = string.Concat(actionMessage, localizer[Text.SelectSettings]);

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: text,
            replyMarkup: keyboard,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectSettings;
    }

    private async Task HandleSelectedSellerSettings(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.ChangeLanguage => SendMenuLanguagesAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.PersonalInfo => SendMenuPersonalInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.CompanyInfo => SendMenuCompanyInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.MessageToDeveloper => SendRequestMessageForDeveloperAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task SendMessageMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
            InputFieldPlaceholder = localizer[Text.AskMessageInPlaceHolder],
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskMessage],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendMessage;
    }

    private async Task SendStatisticsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);
    }

    private async Task SendRequestForUserIdAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskUserIdInPlaceHolder],
        };

        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskUserId],
            replyMarkup: keyboard,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);

        var bot = await botClient.GetMeAsync(cancellationToken: cancellationToken);
        bot.SupportsInlineQueries = true;

        user.State = States.WaitingForSendUserId;
        user.MessageId = sentMessage.MessageId;
    }

    private async Task HandleUserIdAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var bot = await botClient.GetMeAsync(cancellationToken: cancellationToken);
        bot.SupportsInlineQueries = false;
        Domain.Entities.User customer = default!;

        if (message.ViaBot is not null && message.ViaBot.Id.Equals(bot.Id) && long.TryParse(message.Text, out var id))
            customer = (await appDbContext.Users.FirstOrDefaultAsync(user
                    => user.Id.Equals(id),
                cancellationToken: cancellationToken))!;

        if (customer is null)
        {
            await SendSellerMenuAsync(botClient, message, cancellationToken);
            return;
        }

        user.PlaceId = customer.Id;
        await SendUserManagerMenuAsync(botClient, message, cancellationToken);
    }

    private async Task SendUserManagerMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Transaction], CallbackData.Transaction)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Statistics], CallbackData.Statistics)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Settings], CallbackData.Settings)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        });
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
            CallbackData.Settings => SendUserSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => default!
        });
    }

    private async Task SendUserSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);
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
        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);
    }
}
