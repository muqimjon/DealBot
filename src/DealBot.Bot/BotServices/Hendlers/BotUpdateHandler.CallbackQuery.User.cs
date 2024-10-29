namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendMenuLanguagesAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.LanguageUz], CallbackData.CultureUz)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.LanguageEn], CallbackData.Female)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.LanguageRu], CallbackData.CultureRu)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        Message sentMessage = await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: localizer[Text.AskLanguage],
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectLanguage;
    }

    private async Task HandleSelectedLanguageAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message);

        user.LanguageCode = callbackQuery.Data switch
        {
            CallbackData.CultureEn => "en",
            CallbackData.CultureRu => "ru",
            _ => "uz"
        };

        CultureInfo.CurrentCulture = new CultureInfo("uz");
        CultureInfo.CurrentUICulture = new CultureInfo("uz");

        await (user.State switch
        {
            States.WaitingForFirstSelectLanguage => SendGreetingAsync(botClient, callbackQuery.Message, cancellationToken),
            States.WaitingForSelectLanguage => SendCustomerSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => throw new NotImplementedException(),
        });
    }

    private async Task SendMenuPersonalInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.FirstName], CallbackData.FirstName),
                InlineKeyboardButton.WithCallbackData(localizer[Text.LastName], CallbackData.LastName)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.DateOfBirth], CallbackData.DateOfBirth),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Gender], CallbackData.Gender)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.PhoneNumber], CallbackData.PhoneNumber),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Email], CallbackData.Email)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var text = string.Concat(actionMessage, localizer[Text.MenuChangePersonalInfo,
                    user.FirstName,
                    user.LastName,
                    user.DateOfBirth.ToString("yyyy-MM-dd"),
                    localizer[user.Gender.ToString()],
                    user.Contact.Phone!,
                    user.Contact.Email!]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient,
            message,
            text,
            keyboard,
            cancellationToken); 

        user.IsActive = IsAccountComplete(user);
        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectChangePersonalInfo;
    }

    private async Task HandleSelectedChangePersonalInfoAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.FirstName => SendRequestFirstNameAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.LastName => SendRequestLastNameAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.DateOfBirth => SendRequestDateOfBirthAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Gender => SendRequestGenderAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.PhoneNumber => SendRequestPhoneNumberAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Email => SendRequestEmailAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task SendRequestGenderAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Male], CallbackData.Male),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Female], CallbackData.Female)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var text = string.Concat(actionMessage, localizer[Text.AskGender, localizer[user.Gender.ToString()]]);
        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectGender;
    }

    private async Task HandleSelectedGenderAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message);

        user.Gender = callbackQuery.Data switch
        {
            CallbackData.Female => Genders.Female,
            CallbackData.Male => Genders.Male,
            _ => Genders.Unknown,
        };

        await SendMenuPersonalInfoAsync(botClient, callbackQuery.Message, cancellationToken);
    }
}
