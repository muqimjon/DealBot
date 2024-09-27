namespace DealBot.Bot.BotServices;

using Telegram.Bot.Types;
using Telegram.Bot;
using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendCustomerMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.MyPrivilegeCard], CallbackData.MyPrivilegeCard)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.OrderOnTheSite], CallbackData.OrderOnTheSite)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Settings], CallbackData.Settings),
                InlineKeyboardButton.WithCallbackData(localizer[Text.StoreAddress], CallbackData.StoreAddress)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ContactUs], CallbackData.ContactUs),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Comment], CallbackData.Comment)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Referral], CallbackData.Referral)],
        });

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.SelectMenu],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = message.MessageId;
        user.State = States.WaitingForSelecCustomertMenu;
    }

    private async Task HandleSelectedCustomerMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var handler = callbackQuery.Data switch
        {
            CallbackData.MyPrivilegeCard => CheckSubscription(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.OrderOnTheSite => SendOrderOnTheSiteAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.StoreAddress => SendStoreAddressAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.ContactUs => SendContactInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Comment => SendRequestCommentAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Settings => SendCustomerSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling callback query: {callbackQuery.Data}", callbackQuery); }
    }
    private async Task CheckSubscription(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        // TO DO

        await SendUserPrivilegeCardAsync(botClient, message, cancellationToken);
    }

    private async Task SendUserPrivilegeCardAsync(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Subscribe], CallbackData.Subscribe)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Check], CallbackData.Check)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId:message.MessageId,
            text: localizer[Text.AskJoinToChannelForCard],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = message.MessageId;
        user.State = States.WaitingForSubscribeToChannel;
    }

    private async Task HandleSubscribeToChannel(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message);

        await botClient.SendChatActionAsync(
            chatId: callbackQuery.Message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        await (callbackQuery.Data switch
        {
            CallbackData.Check => SendUserPrivilegeCardAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Back => SendCustomerMenuAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken)
        });
    }

    private async Task SendOrderOnTheSiteAsync(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);
    }

    private async Task SendStoreAddressAsync(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);
    }

    private async Task SendContactInfoAsync(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);
    }

    private async Task SendRequestCommentAsync(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);
    }

    private async Task SendCustomerSettingsAsync(ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);
    }
}
