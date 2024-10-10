namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
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

        Task handler;

        if (callbackQuery.Data.Equals(CallbackData.Back))
            handler = NavigateToPreviousPageAsync(botClient, callbackQuery.Message, cancellationToken);
        else
            handler = user.State switch
            {
                States.WaitingForFirstSelectLanguage => HandleSelectedLanguageAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectLanguage => HandleSelectedLanguageAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectMenu => user.Role switch
                {
                    Roles.Customer => HandleSelectedCustomerMenuAsync(botClient, callbackQuery, cancellationToken),
                    Roles.Seller => HandleSelectedSellerMenuAsync(botClient, callbackQuery, cancellationToken),
                    _ => default!,
                },
                States.WaitingForSelectSettings => user.Role switch
                {
                    Roles.Customer => HandleSelectedCustomerSettingsAsync(botClient, callbackQuery, cancellationToken),
                    Roles.Seller => HandleSelectedSellerSettings(botClient, callbackQuery, cancellationToken),
                    _ => default!,
                },
                States.WaitingForSubscribeToChannel => HandleSubscribeToChannel(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectChangePersonalInfo => HandleSelectedChangePersonalInfoAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectGender => HandleSelectedGenderAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectDateOfBirth => HandleDateOfBirthAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectDateOfBirthYear1 => HandleYearAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectDateOfBirthYear2 => HandleYearAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectDateOfBirthYear3 => HandleYearAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectDateOfBirthYear4 => HandleYearAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectDateOfBirthYear5 => HandleYearAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectDateOfBirthMonth => HandleMonthAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectDateOfBirthDay => HandleDayAsync(botClient, callbackQuery, cancellationToken),
                States.WaitingForSelectBotSettings => HandleBotSettings(botClient, callbackQuery, cancellationToken),
                _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
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
            States.WaitingForSelectSettings => user.Role switch
            {
                Roles.Customer => SendCustomerMenuAsync(botClient, message, cancellationToken),
                Roles.Seller => SendSellerMenuAsync(botClient, message, cancellationToken),
                _ => default!,
            },
            States.WaitingForSelectLanguage => user.Role switch
            {
                Roles.Customer => SendCustomerMenuAsync(botClient, message, cancellationToken),
                Roles.Seller => SendSellerSettingsAsync(botClient, message, cancellationToken),
                _ => default!,
            },
            States.WaitingForSelectChangePersonalInfo => user.Role switch
            {
                Roles.Customer => SendCustomerSettingsAsync(botClient, message, cancellationToken),
                Roles.Seller => SendSellerSettingsAsync(botClient, message, cancellationToken),
                _ => default!,
            },
            States.WaitingForSelectGender => SendMenuChangePersonalInfoAsync(botClient, message, cancellationToken),
            States.WaitingForSelectDateOfBirth => SendMenuChangePersonalInfoAsync(botClient, message, cancellationToken),
            States.WaitingForSendEmail => SendMenuChangePersonalInfoAsync(botClient, message, cancellationToken),
            States.WaitingForSendLastName => SendMenuChangePersonalInfoAsync(botClient, message, cancellationToken),
            States.WaitingForSendFirstName => SendMenuChangePersonalInfoAsync(botClient, message, cancellationToken),
            States.WaitingForSelectDateOfBirthYear1 => SendRequestDateOfBirthAsync(botClient, message, cancellationToken),
            States.WaitingForSelectDateOfBirthYear2 => SendRequestDateOfBirthAsync(botClient, message, cancellationToken),
            States.WaitingForSelectDateOfBirthYear3 => SendRequestDateOfBirthAsync(botClient, message, cancellationToken),
            States.WaitingForSelectDateOfBirthYear4 => SendRequestDateOfBirthAsync(botClient, message, cancellationToken),
            States.WaitingForSelectDateOfBirthYear5 => SendRequestDateOfBirthAsync(botClient, message, cancellationToken),
            States.WaitingForSelectDateOfBirthMonth => SendRequestDateOfBirthAsync(botClient, message, cancellationToken),
            States.WaitingForSelectDateOfBirthDay => SendRequestDateOfBirthAsync(botClient, message, cancellationToken),
            States.WaitingForSelectBotSettings => SendSellerSettingsAsync(botClient, message, cancellationToken),
            States.WaitingForSendName => SendBotSettingsAsync(botClient, message, cancellationToken),
            States.WaitingForSendBotPic => SendBotSettingsAsync(botClient, message, cancellationToken),
            States.WaitingForSendAbout => SendBotSettingsAsync(botClient, message, cancellationToken),
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
        try
        {
            await botClient.DeleteMessageAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                cancellationToken: cancellationToken);
        }
        catch { }

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

