﻿namespace DealBot.Bot.BotServices;

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
        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.UserManager], CallbackData.UserManager)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.CustomersList], CallbackData.CustomersList),
                InlineKeyboardButton.WithCallbackData(localizer[Text.EmployeesList], CallbackData.EmployeesList)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.SendMessage], CallbackData.SendMessage),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Statistics], CallbackData.Statistics)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Settings], CallbackData.Settings)],
        ]);

        var text = string.Concat(actionMessage, localizer[Text.SelectMenu]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
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
            CallbackData.SendMessage => SendRequestMessageTextAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Settings => SendAdminSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }


    private async Task SendAdminSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.PersonalInfo], CallbackData.PersonalInfo),
                InlineKeyboardButton.WithCallbackData(localizer[Text.ChangeLanguage], CallbackData.ChangeLanguage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Cashback], CallbackData.Cashback),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Company], CallbackData.Company)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.MessageToDeveloper], CallbackData.SendMessage)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

        var text = string.Concat(actionMessage, localizer[Text.SelectSettings]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
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


    private async Task SendMenuCompanyInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        Store store;
        if ((store = (await appDbContext.Stores
            .Include(s => s.Image)
            .FirstOrDefaultAsync(cancellationToken))!) is null)
            await appDbContext.Stores.AddAsync(store = new());

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Name], CallbackData.Name),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Description], CallbackData.Description)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Picture], CallbackData.Picture),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Website], CallbackData.Website)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Channel], CallbackData.Channel),
                InlineKeyboardButton.WithCallbackData(localizer[Text.MiniAppUrl], CallbackData.MiniAppUrl)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Contact], CallbackData.Contact),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Address], CallbackData.Address)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

        var companyInfo = localizer[
            Text.CompanyInfo,
            localizer[string.IsNullOrEmpty(store.Name) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Description) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Image?.FileId) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Website) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.MiniAppUrl) ? Text.Undefined : Text.Defined],
            localizer[string.IsNullOrEmpty(store.Channel) ? Text.Undefined : Text.Defined]];

        var text = string.Concat(actionMessage, companyInfo, localizer[Text.SelectSettings]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
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
            CallbackData.Contact => SendMenuContactInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Address => SendMenuAddressInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => default!,
        });
    }

    private async Task SendMenuContactInfoAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken,
        string actionMessage = Text.Empty,
        Domain.Entities.Contact contacts = default!)
    {
        if (contacts is null)
        {
            var store = await appDbContext.Stores
            .Include(s => s.Contact)
            .OrderByDescending(s => s.CreatedAt)
            .LastOrDefaultAsync(cancellationToken);

            if (store is null)
            {
                await SendAdminSettingsAsync(botClient, message, cancellationToken, localizer[Text.Error]);
                return;
            }
            else if (store.Contact is null)
                await appDbContext.Contacts.AddAsync(store.Contact ??= new());

            contacts = store.Contact;
        }

        var addressInfo = string.Concat(actionMessage, localizer[Text.ContactInfo,
        string.IsNullOrEmpty(contacts.Phone) ? localizer[Text.Undefined] : contacts.Phone,
        string.IsNullOrEmpty(contacts.Email) ? localizer[Text.Undefined] : contacts.Email]);

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Email], CallbackData.Email),
                InlineKeyboardButton.WithCallbackData(localizer[Text.PhoneNumber], CallbackData.PhoneNumber)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        ]);

        var text = string.Concat(addressInfo, localizer[Text.SelectSettings]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectContactSettings;
    }

    private async Task HandleContactsMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.PhoneNumber => SendRequestCompanyPhoneNumberAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Email => SendRequestForEmailAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => default!,
        });
    }

    private async Task SendMenuAddressInfoAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken,
        string actionMessage = Text.Empty,
        Address address = default!)
    {
        if (address is null)
        {
            var store = await appDbContext.Stores
            .Include(s => s.Address)
            .OrderByDescending(s => s.CreatedAt)
            .LastOrDefaultAsync(cancellationToken);

            if (store is null)
            {
                await SendAdminSettingsAsync(botClient, message, cancellationToken, localizer[Text.Error]);
                return;
            }
            else if (store.Address is null)
                await appDbContext.Addresses.AddAsync(store.Address ??= new());

            address = store.Address;
        }

        var addressInfo = string.Concat(actionMessage, localizer[Text.AddressInfo,
            string.IsNullOrEmpty(address.Country) ? localizer[Text.Undefined] : address.Country,
            string.IsNullOrEmpty(address.CountryCode) ? localizer[Text.Undefined] : address.CountryCode,
            string.IsNullOrEmpty(address.Region) ? localizer[Text.Undefined] : address.Region,
            string.IsNullOrEmpty(address.District) ? localizer[Text.Undefined] : address.District,
            string.IsNullOrEmpty(address.City) ? localizer[Text.Undefined] : address.City,
            string.IsNullOrEmpty(address.Street) ? localizer[Text.Undefined] : address.Street,
            string.IsNullOrEmpty(address.HouseNumber) ? localizer[Text.Undefined] : address.HouseNumber,
            localizer[address.Latitude == 0 || address.Longitude == 0 ? Text.Undefined : Text.Defined]]);

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Location], CallbackData.Location)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Country], CallbackData.Country),
                InlineKeyboardButton.WithCallbackData(localizer[Text.CountryCode], CallbackData.CountryCode)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Region], CallbackData.Region),
                InlineKeyboardButton.WithCallbackData(localizer[Text.City], CallbackData.City)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.District], CallbackData.District),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Street], CallbackData.Street)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.HouseNumber], CallbackData.HouseNumber)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        ]);

        var text = string.Concat(addressInfo, localizer[Text.SelectSettings]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectAddressSettings;
    }

    private async Task HandleAddressSettingsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.Location => SendRequestForLocationAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Country => SendRequestForCountryAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.CountryCode => SendRequestForCountryCodeAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Region => SendRequestForRegionAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.City => SendRequestForCityAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.District => SendRequestForDistrictAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Street => SendRequestForStreetAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.HouseNumber => SendRequestForHouseAsync(botClient, callbackQuery.Message, cancellationToken),
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

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        ]);

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


    private async Task SendRequestRoleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, Domain.Entities.User value = default!)
    {
        value ??= user;
        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Seller], CallbackData.Seller),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Customer], CallbackData.Customer)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

        var text = string.Concat(localizer[Text.AskRole, localizer[value.Role.ToString()]]);
        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectRole;
    }

    private async Task HandleSelectedRoleAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message);

        var value = user;
        if (user.PlaceId != 0 && (value = await appDbContext.Users.FirstOrDefaultAsync(u => u.Id == user.PlaceId, cancellationToken)) is null)
        {
            await SendAdminMenuAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.Error]);
            return;
        }

        value.Role = callbackQuery.Data switch
        {
            CallbackData.Seller => Roles.Seller,
            _ => Roles.Customer,
        };

        await SendMenuPersonalInfoAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.UpdateSucceeded]);
    }


    private async Task SendAdminUserSettingsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        var value = user;
        if (user.PlaceId != 0)
            if ((value = await appDbContext.Users
                .Include(u => u.Contact)
                .FirstOrDefaultAsync(u => u.Id == user.PlaceId, cancellationToken)) is null)
            {
                await SendAdminMenuAsync(botClient, message, cancellationToken, localizer[Text.Error]);
                return;
            }

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.FirstName], CallbackData.FirstName),
                InlineKeyboardButton.WithCallbackData(localizer[Text.LastName], CallbackData.LastName)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.DateOfBirth], CallbackData.DateOfBirth),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Gender], CallbackData.Gender)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.PhoneNumber], CallbackData.PhoneNumber),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Email], CallbackData.Email)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Role], CallbackData.Role)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

        value.Contact ??= new();

        var text = string.Concat(actionMessage,
            localizer[
                Text.AdminMenuPersonalInfo,
                value.FirstName,
                value.LastName,
                value.DateOfBirth.ToString("yyyy-MM-dd"),
                localizer[value.Gender.ToString()],
                value.Contact.Phone!,
                value.Contact.Email!,
                localizer[value.Role.ToString()]]
            );

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectUserInfo;
    }
}
