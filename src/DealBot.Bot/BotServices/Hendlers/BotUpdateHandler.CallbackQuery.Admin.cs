namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Entities;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text;
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
            [InlineKeyboardButton.WithCallbackData(localizer[Text.UserManager], CallbackData.UserManager)],
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

        user.PlaceId = 0;
        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectMenu;
    }

    private async Task HandleSelectedAdminMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.UserManager => SendRequestForUserIdAsync(botClient, callbackQuery.Message, cancellationToken),
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
            [InlineKeyboardButton.WithCallbackData(localizer[Text.PersonalInfo], CallbackData.PersonalInfo),
                InlineKeyboardButton.WithCallbackData(localizer[Text.ChangeLanguage], CallbackData.ChangeLanguage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Cashback], CallbackData.Cashback),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Company], CallbackData.Company)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.MessageToDeveloper], CallbackData.SendMessage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var text = string.Concat(actionMessage, localizer[Text.SelectSettings]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectSettings;
    }

    private async Task HandleSelectedAdminSettingsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.ChangeLanguage => SendMenuLanguagesAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.PersonalInfo => SendMenuPersonalInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Company => SendMenuCompanyInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Role => SendMenuCompanyInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Cashback => SendRequestSimpleCashbackQuantityAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.SendMessage => SendRequestMessageToDeveloperAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task SendCashbackSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty, CashbackSetting settings = default!)
    {
        var isSimple = settings is not null && settings.Type is CardTypes.Simple;
        var isPremium = settings is not null && settings.Type is CardTypes.Premium;

        var premiumCS = isPremium ? settings : null
            ?? await appDbContext.CashbackSettings
            .OrderByDescending(cs => cs.Id)
            .FirstOrDefaultAsync(cs
            => cs.Type == CardTypes.Premium
            && cs.IsActive,
            cancellationToken);

        if (premiumCS is null)
            await appDbContext.CashbackSettings.AddAsync(premiumCS = new()
            {
                Type = CardTypes.Premium,
                IsActive = true,
            }, cancellationToken);

        var simpleCS = isSimple ? settings : null
            ?? await appDbContext.CashbackSettings
            .OrderByDescending(cs => cs.Id)
            .FirstOrDefaultAsync(cs
            => cs.Type == CardTypes.Simple
            && cs.IsActive,
            cancellationToken);

        if (simpleCS is null)
            await appDbContext.CashbackSettings.AddAsync(simpleCS = new()
            {
                Type = CardTypes.Simple,
                IsActive = true,
            }, cancellationToken);

        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Simple], CallbackData.Simple)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Premium], CallbackData.Premium)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var text = string.Concat(
            actionMessage,
            localizer[
                Text.CashbackInfo,
                simpleCS.Percentage.ToString("P0"),
                premiumCS.Percentage.ToString("P0")]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectCardType;
    }

    private async Task HandleSelectedCardTypeAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.Simple => SendRequestSimpleCashbackQuantityAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Premium => SendRequestPremuimCashbackQuantityAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => default!,
        });
    }

    private async Task SendRequestPremuimCashbackQuantityAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, CashbackSetting template = default!)
    {
        var cashbackSettings = appDbContext.CashbackSettings.Where(cs => cs.Type == CardTypes.Premium);

        var actualCS = await cashbackSettings
            .OrderByDescending(cs => cs.Id)
            .FirstOrDefaultAsync(cs => cs.IsActive, cancellationToken);

        template ??= (await cashbackSettings
            .OrderByDescending(cs => cs.Id)
            .FirstOrDefaultAsync(cs => !cs.IsActive, cancellationToken))!;

        if (template is null)
            await appDbContext.CashbackSettings.AddAsync(template = new() { Type = CardTypes.Premium }, cancellationToken);

        actualCS ??= new();

        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData("<<", CallbackData.Previous2),
                InlineKeyboardButton.WithCallbackData("<", CallbackData.Previous),
                InlineKeyboardButton.WithCallbackData(">", CallbackData.Next),
                InlineKeyboardButton.WithCallbackData(">>", CallbackData.Next2)],
                [InlineKeyboardButton.WithCallbackData(localizer[Text.Submit], CallbackData.Submit)],
                [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var text = string.Concat(
            localizer[
                Text.CashbackChanged,
                localizer[actualCS.Type.ToString()],
                actualCS.Percentage.ToString("P0"),
                template.Percentage.ToString("P0")]);

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectCashbackQuantityPremium;
    }

    private async Task HandleCashbackQuantityPremiumAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        var template = await appDbContext.CashbackSettings
            .OrderByDescending(cs => cs.Id)
            .FirstOrDefaultAsync(cs
            => !cs.IsActive && cs.Type == CardTypes.Premium,
            cancellationToken);

        if (template is null)
        {
            await SendAdminSettingsAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.Error]);
            return;
        }
        else if (callbackQuery.Data == CallbackData.Submit)
        {
            template.IsActive = true;
            await SendAdminSettingsAsync(
                botClient,
                callbackQuery.Message,
                cancellationToken,
                localizer[Text.UpdateSucceeded]);
            return;
        }
        else if (callbackQuery.Data == CallbackData.Cancel)
        {
            appDbContext.CashbackSettings.Remove(template);
            await SendAdminSettingsAsync(botClient, callbackQuery.Message, cancellationToken);
            return;
        }

        template.Percentage += callbackQuery.Data switch
        {
            CallbackData.Previous2 => -0.1m,
            CallbackData.Previous => -0.01m,
            CallbackData.Next => 0.01m,
            CallbackData.Next2 => 0.1m,
            _ => 0
        };

        await SendRequestPremuimCashbackQuantityAsync(botClient, callbackQuery.Message, cancellationToken, template);
    }

    private async Task SendRequestSimpleCashbackQuantityAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, CashbackSetting template = default!)
    {
        var cashbackSettings = appDbContext.CashbackSettings.Where(cs => cs.Type == CardTypes.Simple);

        var actualCS = await cashbackSettings
            .OrderByDescending(cs => cs.Id)
            .FirstOrDefaultAsync(cs => cs.IsActive, cancellationToken) ?? new();

        template ??= (await cashbackSettings
            .OrderByDescending(cs => cs.Id)
            .FirstOrDefaultAsync(cs => !cs.IsActive, cancellationToken))!;

        if (template is null)
            await appDbContext.CashbackSettings.AddAsync(template = new() { Type = CardTypes.Simple }, cancellationToken);

        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData("<<", CallbackData.Previous2),
                InlineKeyboardButton.WithCallbackData("<", CallbackData.Previous),
                InlineKeyboardButton.WithCallbackData(">", CallbackData.Next),
                InlineKeyboardButton.WithCallbackData(">>", CallbackData.Next2)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Cancel], CallbackData.Cancel),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Submit], CallbackData.Submit)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var text = string.Concat(
            localizer[
                Text.CashbackChanged,
                localizer[actualCS.Type.ToString()],
                actualCS.Percentage.ToString("P0"),
                template.Percentage.ToString("P0")]);

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectCashbackQuantitySimple;
    }

    private async Task HandleCashbackQuantitySimpleAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        var template = await appDbContext.CashbackSettings
            .OrderByDescending(cs => cs.Id)
            .FirstOrDefaultAsync(cs
            => !cs.IsActive && cs.Type == CardTypes.Simple,
            cancellationToken);

        if (template is null)
        {
            await SendAdminSettingsAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.Error]);
            return;
        }
        else if (callbackQuery.Data == CallbackData.Submit)
        {
            template.IsActive = true;
            await SendAdminSettingsAsync(
                botClient,
                callbackQuery.Message,
                cancellationToken,
                localizer[Text.UpdateSucceeded]);
            return;
        }
        else if (callbackQuery.Data == CallbackData.Cancel)
        {
            appDbContext.CashbackSettings.Remove(template);
            await SendAdminSettingsAsync(botClient, callbackQuery.Message, cancellationToken);
            return;
        }

        template.Percentage += callbackQuery.Data switch
        {
            CallbackData.Previous2 => -0.1m,
            CallbackData.Previous => -0.01m,
            CallbackData.Next => 0.01m,
            CallbackData.Next2 => 0.1m,
            _ => 0
        };

        await SendRequestSimpleCashbackQuantityAsync(botClient, callbackQuery.Message, cancellationToken, template);
    }

    private async Task SendMenuCompanyInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        Store store;
        if ((store = (await appDbContext.Stores
            .Include(s => s.Image)
            .Include(s => s.Contact)
            .FirstOrDefaultAsync(cancellationToken))!) is null)
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
            localizer[string.IsNullOrEmpty(store.Image?.FileId) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Contact?.Phone) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Contact?.Email) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Website) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.MiniAppUrl) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Channel) ? Text.Undefined : Text.Defined]];

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

    private async Task SendEmployeesListAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var customers = appDbContext.Users
            .Include(u => u.Contact)
            .Where(u => u.Role == Roles.Seller);

        StringBuilder text = new();

        foreach (var customer in customers)
            text.Append($"_{customer.GetFullName()}_ \\{customer.Contact.Phone}\n");

        var xabar = string.IsNullOrEmpty(text.ToString()) ? "Xodimlar mavjud emas mavjud emas" : text.ToString();

        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        });

        await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: xabar,
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = message.MessageId;
        user.State = States.EmployeesList;
    }
}
