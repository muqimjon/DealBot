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
    private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Data);
        ArgumentNullException.ThrowIfNull(callbackQuery.Message);

        if (callbackQuery.Data.Equals(CallbackData.Back))
            await NavigateToPreviousPageAsync(botClient, callbackQuery.Message, cancellationToken);

        var handler = user.State switch
        {
            States.WaitingForFirstSelectLanguage => HandleSelectedLanguageAsync(botClient, callbackQuery, cancellationToken),
            States.WaitingForSelectLanguage => HandleSelectedLanguageAsync(botClient, callbackQuery, cancellationToken),
            States.WaitingForSelecCustomertMenu => HandleSelectedCustomerMenuAsync(botClient, callbackQuery, cancellationToken),
            States.WaitingForSubscribeToChannel => HandleSubscribeToChannel(botClient, callbackQuery, cancellationToken),
            States.WaitingForSelectCustomerSettings => HandleSelectedCustomerSettingsAsync(botClient, callbackQuery, cancellationToken),
            States.WaitingForSelectMenuChangeCustomerInfo => HandleSelectedMenuChangePersonalInfoAsync(botClient, callbackQuery, cancellationToken),
            States.WaitingForSelectGender => HandleSelectedGenderAsync(botClient, callbackQuery, cancellationToken),
            States.WaitingForSelectDateOfBirth => HandleDateOfBirthAsync(botClient, callbackQuery, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken)
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling callback query: {callbackQuery.Data}", callbackQuery); }
    }

    private async Task NavigateToPreviousPageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var handler = user.State switch
        {
            States.WaitingForSubscribeToChannel => SendCustomerMenuAsync(botClient, message, cancellationToken),
            States.WaitingForSelectCardOption => SendCustomerMenuAsync(botClient, message, cancellationToken),
            States.WaitingForSelectAddressOption => SendCustomerMenuAsync(botClient, message, cancellationToken),
            States.WaitingForSelectStoreContactOption => SendCustomerMenuAsync(botClient, message, cancellationToken),
            States.WaitingForSendComment => SendCustomerMenuAsync(botClient, message, cancellationToken),
            States.WaitingForSelectCustomerSettings => SendCustomerMenuAsync(botClient, message, cancellationToken),
            States.WaitingForSelectMenuChangeCustomerInfo => SendCustomerSettingsAsync(botClient, message, cancellationToken),
            States.WaitingForSelectLanguage => SendCustomerMenuAsync(botClient, message, cancellationToken),
            States.WaitingForSelectGender => SendMenuChangeCustomerInfoAsync(botClient, message, cancellationToken),
            States.WaitingForSelectDateOfBirth => SendMenuChangeCustomerInfoAsync(botClient, message, cancellationToken),
            States.WaitingForSendEmail => SendMenuChangeCustomerInfoAsync(botClient, message, cancellationToken),
            States.WaitingForSendLastName => SendMenuChangeCustomerInfoAsync(botClient, message, cancellationToken),
            States.WaitingForSendFirstName => SendMenuChangeCustomerInfoAsync(botClient, message, cancellationToken),
            _ => HandleUnknownMessageAsync(botClient, message, cancellationToken),
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling callback query: {message.Text}", message); }
    }

    private Task HandleUnknownCallbackQueryAsync(ITelegramBotClient _, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received unknown callback query: {callbackQuery.Data}", callbackQuery?.Data);
        return Task.CompletedTask;
    }

    private async Task SendFirstMenuLanguagesAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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

        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskLanguage],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForFirstSelectLanguage;
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

