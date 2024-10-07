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
    private async Task SendCustomerSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ChangeLanguage], CallbackData.ChangeLanguage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ChangePersonalInfo], CallbackData.ChangePersonalInfo)],
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

    private async Task HandleSelectedCustomerSettingsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.ChangeLanguage => SendMenuLanguagesAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.ChangePersonalInfo => SendMenuChangeCustomerInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

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

    private async Task SendMenuChangeCustomerInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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

        Message sentMessage = default!;

        try
        {
            sentMessage = await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: localizer[Text.MenuChangePersonalInfo,
                    user.FirstName,
                    user.LastName,
                    user.DateOfBirth.ToString("yyyy-MM-dd"),
                    user.Gender,
                    user.Contact.Phone!,
                    user.Contact.Email!],
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
                    text: localizer[Text.MenuChangePersonalInfo,
                        user.FirstName,
                        user.LastName,
                        user.DateOfBirth.ToString("yyyy-MM-dd"),
                        user.Gender,
                        user.Contact.Phone!,
                        user.Contact.Email!],
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);

                await botClient.DeleteMessageAsync(
                    messageId: message.MessageId,
                    chatId: message.Chat.Id,
                    cancellationToken: cancellationToken);

                await botClient.DeleteMessageAsync(
                    messageId: user.MessageId,
                    chatId: message.Chat.Id,
                    cancellationToken: cancellationToken);
            }
            catch { }
        }

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectMenuChangePersonalInfo;
    }

    private async Task HandleSelectedMenuChangePersonalInfoAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
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

    private async Task SendRequestGenderAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Male], CallbackData.Male),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Female], CallbackData.Female)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        Message sentMessage = await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: localizer[Text.AskGender, localizer[user.Gender.ToString()]],
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

        await SendMenuChangeCustomerInfoAsync(botClient, callbackQuery.Message, cancellationToken);
    }
}
