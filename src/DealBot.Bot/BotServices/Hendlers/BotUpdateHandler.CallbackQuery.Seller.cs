namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
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
            [InlineKeyboardButton.WithCallbackData(localizer[Text.SendMessage], CallbackData.SendMessage),
                InlineKeyboardButton.WithCallbackData(localizer[Text.GiveCashBack], CallbackData.GiveCashBack)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Statistics], CallbackData.Statistics),
                InlineKeyboardButton.WithCallbackData(localizer[Text.CustomersList], CallbackData.CustomersList)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Settings], CallbackData.Settings)],
        });

        Message sentMessage = default!;
        var text = string.Concat(messageText,
            localizer[Text.SelectMenu]);

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
        user.State = States.WaitingForSelectMenu;
    }

    private async Task HandleSelectedSellerMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.CustomersList => SendCustomerListAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.GiveCashBack => SendGiveCashbackAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Statistics => SendStatisticsAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.SendMessage => SendMessageMenuAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Settings => SendSellerSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task SendSellerSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ChangePersonalInfo], CallbackData.ChangePersonalInfo),
                InlineKeyboardButton.WithCallbackData(localizer[Text.ChangeCompanySettings], CallbackData.ChangeCompanySettings)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ChangeLanguage], CallbackData.ChangeLanguage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: localizer[Text.SelectSettings],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectSettings;
    }

    private async Task HandleSelectedSellerSettings(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.ChangePersonalInfo => SendMenuChangePersonalInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.ChangeCompanySettings => SendMenuChangeCompanyInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.ChangeLanguage => SendMenuLanguagesAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task SendMenuChangeCompanyInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);
    }

    private async Task SendMessageMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);
    }

    private async Task SendStatisticsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);
    }

    private async Task SendGiveCashbackAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
