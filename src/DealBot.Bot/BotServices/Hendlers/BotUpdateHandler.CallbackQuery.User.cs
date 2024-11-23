namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendCustomerSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ChangeLanguage], CallbackData.ChangeLanguage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.PersonalInfo], CallbackData.PersonalInfo)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var text = string.Concat(actionMessage, localizer[Text.SelectSettings]);

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: text,
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
            CallbackData.PersonalInfo => SendMenuPersonalInfoAsync(botClient, callbackQuery.Message, cancellationToken),
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


    private async Task SendMenuPersonalInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        if (user.PlaceId != 0)
        {
            await (user.Role switch
            {
                Roles.Admin => SendAdminUserSettingsAsync(botClient, message, cancellationToken, actionMessage),
                Roles.Seller => SendSellerUserSettingsAsync(botClient, message, cancellationToken, actionMessage),
                _ => default!
            });

            return;
        }

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

        var text = string.Concat(actionMessage, localizer[Text.MenuPersonalInfo,
            user.FirstName,
            user.LastName,
            user.DateOfBirth.ToString("yyyy-MM-dd"),
            localizer[user.Gender.ToString()],
            user.Contact.Phone!,
            user.Contact.Email!]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.IsActive = IsAccountComplete(user);
        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectUserInfo;
    }

    private async Task HandleSelectedUserInfoAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        Domain.Entities.User? value = default!;
        if (user.PlaceId != 0)
        {
            value = await appDbContext.Users
                .Include(u => u.Contact)
                .FirstOrDefaultAsync(u => u.Id == user.PlaceId, cancellationToken);

            if (value is null)
            {
                await SendAdminMenuAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.Error]);
                return;
            }
        }

        await (callbackQuery.Data switch
        {
            CallbackData.FirstName => SendRequestFirstNameAsync(botClient, callbackQuery.Message, cancellationToken, value),
            CallbackData.LastName => SendRequestLastNameAsync(botClient, callbackQuery.Message, cancellationToken, value),
            CallbackData.DateOfBirth => SendRequestDateOfBirthAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Gender => SendRequestGenderAsync(botClient, callbackQuery.Message, cancellationToken, value: value),
            CallbackData.PhoneNumber => SendRequestPhoneNumberAsync(botClient, callbackQuery.Message, cancellationToken, value),
            CallbackData.Email => SendRequestEmailAsync(botClient, callbackQuery.Message, cancellationToken, value),
            CallbackData.Role => SendRequestRoleAsync(botClient, callbackQuery.Message, cancellationToken, value),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }


    private async Task SendRequestGenderAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty, Domain.Entities.User value = default!)
    {
        value ??= user;
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Male], CallbackData.Male),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Female], CallbackData.Female)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var text = string.Concat(actionMessage, localizer[Text.AskGender, localizer[value.Gender.ToString()]]);
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
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        var prepareMessage = await appDbContext.MyMessages
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync(m => m.Status == Status.Pending, cancellationToken);

        if (prepareMessage is null)
            await appDbContext.MyMessages.AddAsync(prepareMessage = new() { Status = Status.Pending }, cancellationToken);

        var actionMessage = localizer[prepareMessage.Gender == Genders.Unknown ? Text.SetSucceeded : Text.UpdateSucceeded];
        prepareMessage.Gender = callbackQuery.Data switch
        {
            CallbackData.Male => Genders.Male,
            CallbackData.Female => Genders.Female,
            _ => Genders.Unknown,
        };

        await SendMessageMenuAsync(botClient, callbackQuery.Message, cancellationToken, actionMessage);
    }


    private async Task SendSellerUserSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        var value = user;
        if (user.PlaceId != 0)
            if ((value = await appDbContext.Users.FirstOrDefaultAsync(u => u.Id == user.PlaceId, cancellationToken)) is null)
            {
                await SendAdminMenuAsync(botClient, message, cancellationToken, localizer[Text.Error]);
                return;
            }

        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.FirstName], CallbackData.FirstName),
                InlineKeyboardButton.WithCallbackData(localizer[Text.LastName], CallbackData.LastName)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.DateOfBirth], CallbackData.DateOfBirth),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Gender], CallbackData.Gender)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var text = string.Concat(actionMessage,
            localizer[
                Text.SellerMenuPersonalInfo,
                value.FirstName,
                value.LastName,
                value.DateOfBirth.ToString("yyyy-MM-dd"),
                localizer[value.Gender.ToString()],
                localizer[value.Role.ToString()]]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectUserInfo;
    }
    // Handler in admin file
}
