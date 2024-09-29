namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery);
        logger.LogInformation("Received Callback query from {FirstName}", user);

        var handler = user.State switch
        {
            States.WaitingForSelectLanguage => HandleSelectedLanguageAsync(botClient, callbackQuery, cancellationToken),
            States.WaitingForSelecCustomertMenu => HandleSelectedCustomerMenuAsync(botClient, callbackQuery, cancellationToken),
            States.WaitingForSelectCardOption => HandleSelectCardOptionAsync(botClient, callbackQuery, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken)
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling callback query: {callbackQuery.Data}", callbackQuery); }
    }

    private Task HandleUnknownCallbackQueryAsync(ITelegramBotClient _, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received unknown callback query: {callbackQuery.Data}", callbackQuery?.Data);
        return Task.CompletedTask;
    }

    private async Task SendLanguagesMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(Text.LanguageUz, CallbackData.CultureUz)],
            [InlineKeyboardButton.WithCallbackData(Text.LanguageEn, CallbackData.CultureEn)],
            [InlineKeyboardButton.WithCallbackData(Text.LanguageRu, CallbackData.CultureRu)]
        });

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskLanguage],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = message.MessageId;
        user.State = States.WaitingForSelectLanguage;
    }

    private async Task HandleSelectedLanguageAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Data);
        ArgumentNullException.ThrowIfNull(callbackQuery.Message);

        user.LanguageCode = callbackQuery.Data switch
        {
            CallbackData.CultureEn => "en",
            CallbackData.CultureRu => "ru",
            _ => "uz"
        };

        CultureInfo.CurrentCulture = new CultureInfo(user.LanguageCode);
        CultureInfo.CurrentUICulture = new CultureInfo(user.LanguageCode);

        await SendGreetingAsync(botClient, callbackQuery.Message, cancellationToken);
    }

    private async Task SendGreetingAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.SendPhoneNumber]) { RequestContact = true }]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = "Telefon raqamingizni kiriting..",
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.Greeting, user.FirstName, user.LastName],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            messageId: message.MessageId,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendPhoneNumber;
    }
}

