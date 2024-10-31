namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Entities;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendAdminMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.UserManager], CallbackData.UserMamager)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.CustomersList], CallbackData.CustomersList),
                InlineKeyboardButton.WithCallbackData(localizer[Text.EmployeesList], CallbackData.EmployeesList)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.SendMessage], CallbackData.SendMessage),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Statistics], CallbackData.Statistics)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Settings], CallbackData.Settings)],
        });

        var text = string.Concat(actionMessage, localizer[Text.SelectMenu]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectMenu;
    }

    private async Task HandleSelectedAdminMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.UserMamager => SendRequestForUserIdAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.CustomersList => SendCustomerListAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.EmployeesList => SendEmployeesListAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Statistics => SendStatisticsAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.SendMessage => SendMessageMenuAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Settings => SendAdminSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task SendAdminSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ChangeLanguage], CallbackData.ChangeLanguage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.PersonalInfo], CallbackData.PersonalInfo),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Company], CallbackData.Company)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Cashback], CallbackData.Cashback),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Roles], CallbackData.Roles)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.MessageToDeveloper], CallbackData.SendMessage)],
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

    private async Task HandleSelectedAdminSettings(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.ChangeLanguage => SendMenuLanguagesAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.PersonalInfo => SendMenuPersonalInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Company => SendMenuCompanyInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Roles => SendMenuCompanyInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Cashback => SendCashbackSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.SendMessage => SendRequestMessageToDeveloperAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task SendCashbackSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var cashbackSettings = await (from cs in appDbContext.CashbackSettings
                                      orderby cs.UpdatedAt descending
                                      select cs)
                                   .FirstAsync(cancellationToken);

        if (cashbackSettings.IsActive)
            cashbackSettings = new CashbackSetting() { Percentage = cashbackSettings.Percentage, };


        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ChangeLanguage], CallbackData.ChangeLanguage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.PersonalInfo], CallbackData.PersonalInfo),
                InlineKeyboardButton.WithCallbackData(localizer[Text.CompanyInfo], CallbackData.CompanyInfo)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Cashback], CallbackData.Cashback),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Roles], CallbackData.Roles)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.MessageToDeveloper], CallbackData.SendMessage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });
    }

    private async Task SendMenuCompanyInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        Store store;
        if ((store = (await appDbContext.Stores.FirstOrDefaultAsync(cancellationToken))!) is null)
            await appDbContext.Stores.AddAsync(store = new());

        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Name], CallbackData.Name),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Description], CallbackData.Description)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Picture], CallbackData.Picture),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Website], CallbackData.Website)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Channel], CallbackData.Channel),
                InlineKeyboardButton.WithCallbackData(localizer[Text.MiniAppUrl], CallbackData.MiniAppUrl)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.PhoneNumber], CallbackData.PhoneNumber),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Email], CallbackData.Email)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var companyInfo = localizer[
            Text.CompanyInfo,
            localizer[string.IsNullOrEmpty(store.Name) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Description) ? Text.Undefined : Text.Defined],
            localizer[store.Image is null ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Website) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.MiniAppUrl) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Contact?.Phone) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Contact?.Email) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Channel) ? Text.Undefined : Text.Defined]
            ];

        var text = string.Concat(actionMessage, companyInfo, localizer[Text.SelectSettings]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectCompanySettings;
    }

    private async Task HandleCompanySettingsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.Picture => SendRequestForPictureAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Name => SendRequestForNameAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Description => SendRequestForDescriptionAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.MiniAppUrl => SendRequestForMiniAppUrlAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Website => SendRequestForWebsiteAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Channel => SendRequestForChannelAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.PhoneNumber => SendRequestForPhoneNumberAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Email => SendRequestForEmailAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => default!,
        });
    }

    private Task SendEmployeesListAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
