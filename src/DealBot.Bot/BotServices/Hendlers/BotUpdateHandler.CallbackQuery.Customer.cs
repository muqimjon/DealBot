﻿namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Entities;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendCustomerMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        var store = await appDbContext.Stores
            .Include(s => s.Contact)
            .Include(s => s.Address)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!IsStoreReady(store))
        {
            await SendNotReadyNotificationAsync(botClient, message, cancellationToken);
            return;
        }

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.MyPrivilegeCard], CallbackData.MyPrivilegeCard)],
            string.IsNullOrEmpty(store!.MiniAppUrl) ? [] :
                [InlineKeyboardButton.WithWebApp(localizer[Text.OrderOnTheSite], new WebAppInfo() { Url = store.MiniAppUrl })],
            !IsCOntactValid(store.Contact) ? [] :
                [InlineKeyboardButton.WithCallbackData(localizer[Text.ContactUs], CallbackData.ContactUs)],
            !IsAddressValid(store.Address) ? [] :
                [InlineKeyboardButton.WithCallbackData(localizer[Text.Address], CallbackData.Address)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Settings], CallbackData.Settings),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Comment], CallbackData.Comment)],
            [InlineKeyboardButton.WithUrl(localizer[Text.Referral], await GetShareLink(botClient, cancellationToken))],
        ]);

        user.Card.State = await IsSubscribed(botClient, user.TelegramId, cancellationToken)
            ? CardStates.Active : CardStates.Block;

        var text = string.Concat(actionMessage,
            localizer[
                Text.UserInfo,
                user.GetFullName(),
                user.Card.Ballance,
                user.Card.Type],
            localizer[Text.SelectMenu]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectMenu;
    }

    private async Task HandleSelectedCustomerMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.MyPrivilegeCard =>
                await IsSubscribed(botClient, callbackQuery.From.Id, cancellationToken) switch
                {
                    true => SendUserPrivilegeCardAsync(botClient, callbackQuery.Message, cancellationToken),
                    _ => SendRequestJoinToChannel(botClient, callbackQuery.Message, cancellationToken),
                },
            CallbackData.Address => SendStoreAddressAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.ContactUs => SendContactInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Comment => SendRequestCommentAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Settings => SendCustomerSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }


    private async Task SendRequestJoinToChannel(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty, Domain.Entities.User customer = default!)
    {
        var store = await (from s in appDbContext.Stores
                           orderby s.Id descending
                           select s)
                    .LastAsync(cancellationToken);

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithUrl(localizer[Text.Subscribe], store.Channel)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Check], CallbackData.Check)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

        var askToJoin = !user.State.Equals(States.WaitingForSubscribeToChannel)
            ? Text.AskJoinToChannel : Text.AskJoinToChannelAgain;

        var text = string.Concat(actionMessage, localizer[askToJoin]);
        Message sentMessage = default!;

        if (customer is not null)
        {
            await botClient.SendChatActionAsync(
                chatId: customer.ChatId,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            try
            {
                await botClient.DeleteMessageAsync(
                    chatId: customer.ChatId,
                    messageId: customer.MessageId,
                    cancellationToken: cancellationToken);
            }
            catch { }

            sentMessage = await botClient.SendTextMessageAsync(
                chatId: customer.ChatId,
                text: text,
                //parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

            customer.MessageId = sentMessage.MessageId;
            customer.State = States.WaitingForSubscribeToChannel;
            return;
        }

        sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
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
            CallbackData.Check =>
                await IsSubscribed(botClient, callbackQuery.From.Id, cancellationToken) switch
                {
                    true => SendUserPrivilegeCardAsync(botClient, callbackQuery.Message!, cancellationToken),
                    _ => SendRequestJoinToChannel(botClient, callbackQuery.Message!, cancellationToken),
                },
            CallbackData.Back => SendCustomerMenuAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken)
        });
    }

    private async Task SendUserPrivilegeCardAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

        var card = user.Card;

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: localizer[Text.CardInfo, card.Ballance, card.Type, card.State],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectCardOption;
    }

    private async Task SendStoreAddressAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var storeWithAddress = await appDbContext.Stores
            .Include(s => s.Address)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var address = storeWithAddress?.Address;

        if (address is null)
        {
            await SendCustomerMenuAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var addressText = localizer[
            Text.StoreAddress,
            string.IsNullOrWhiteSpace(address.Country) ? localizer[Text.Undefined] : address.Country,
            string.IsNullOrWhiteSpace(address.Region) ? localizer[Text.Undefined] : address.Region,
            string.IsNullOrWhiteSpace(address.District) ? localizer[Text.Undefined] : address.District,
            string.IsNullOrWhiteSpace(address.City) ? localizer[Text.Undefined] : address.City,
            string.IsNullOrWhiteSpace(address.Street) ? localizer[Text.Undefined] : address.Street,
            string.IsNullOrWhiteSpace(address.HouseNumber) ? localizer[Text.Undefined] : address.HouseNumber];

        var inlineKeyboard = new InlineKeyboardMarkup(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ViewDirection], CallbackData.ViewDirection)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        ]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: addressText,
            cancellationToken: cancellationToken,
            keyboard: inlineKeyboard);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectAddressMenu;
    }

    private async Task HandleSelectedAddressMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.ViewDirection => SendDirectionAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }


    private async Task SendDirectionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var address = (await appDbContext.Stores
            .Include(s => s.Address)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken))?.Address;

        if (address is null)
        {
            await SendCustomerMenuAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        string formattedLatitude = address.Latitude.ToString(CultureInfo.InvariantCulture);
        string formattedLongitude = address.Longitude.ToString(CultureInfo.InvariantCulture);

        var addressName = string.Join(", ", [address.Street, address.District, address.Region]);

        // Generate URLs for different map services https://maps.google.com/?q=<lat>,<lng>
        string googleMapsUrl = $"https://maps.google.com/?q={formattedLatitude},{formattedLongitude}";
        string yandexMapsUrl = $"https://yandex.uz/maps/?ll={formattedLongitude}%2C{formattedLatitude}&mode=routes&rtext=~40.244164%2C71.558187&rtt=auto&ruri=~&z=16";
        string yandexMapsUr = $"https://yandex.ru/maps/?ll={formattedLongitude},{formattedLatitude}&z=12&l=map";
        string appleMapsUrl = $"https://maps.apple.com/?q={formattedLatitude},{formattedLongitude}";
        string twoGisUrl = $"https://2gis.com/?query={formattedLatitude}%2C{formattedLongitude}";

        // Inline keyboard buttons for the map services
        var inlineKeyboard = new InlineKeyboardMarkup(
        [
            [InlineKeyboardButton.WithUrl("Google Maps", googleMapsUrl),
                InlineKeyboardButton.WithUrl("Yandex Maps", yandexMapsUrl)],
            [InlineKeyboardButton.WithUrl("Apple Maps", appleMapsUrl),
                InlineKeyboardButton.WithUrl("2GIS", twoGisUrl)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        ]);

        try
        {
            var locationMessage = await botClient.SendLocationAsync(
                chatId: message.Chat.Id,
                latitude: address.Latitude,
                longitude: address.Longitude,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);

            await botClient.DeleteMessageAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                cancellationToken: cancellationToken);

            user.MessageId = locationMessage.MessageId;
            user.State = States.WaitingForSelectDirectionOption;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Manzilga yo'l ko'rsatishda xatolik yuz berdi");
            await SendCustomerMenuAsync(botClient, message, cancellationToken, localizer[Text.Error]);
        }
    }


    private async Task SendContactInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var store = await appDbContext.Stores
            .Include(s => s.Contact)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null || store.Contact is null || store.Contact.Phone is null || store.Contact.Email is null)
        {
            await SendCustomerMenuAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: localizer[Text.StoreContactInfo, store.Contact.Phone, store.Contact.Email],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectStoreContactOption;
    }

    private async Task<bool> IsSubscribed(ITelegramBotClient botClient, long userId, CancellationToken cancellationToken)
    {
        var channel = (await appDbContext.Stores
            .OrderByDescending(s => s.Id)
            .LastOrDefaultAsync(cancellationToken))?.Channel;

        if (channel is null)
            return true;

        try
        {
            var member = await botClient.GetChatMemberAsync(
                chatId: channel,
                userId,
                cancellationToken: cancellationToken);

            return (int)member.Status < 4;
        }
        catch { logger.LogInformation("Invalid username"); }

        return true;
    }

    private async Task<string> GetShareLink(ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        var botInfo = await botClient.GetMeAsync(cancellationToken: cancellationToken);

        return $"https://t.me/share/url?url=https://t.me/{botInfo.Username}&text={localizer[Text.ShortReferralInfo]}";
    }

    private async Task SendCustomerConfirmation(ITelegramBotClient botClient, decimal price, Domain.Entities.User customer, CancellationToken cancellationToken)
    {
        await appDbContext.Transactions.AddAsync(new()
        {
            Amount = price,
            Status = Status.Pending,
            Customer = customer,
            Seller = user,
            IsCashback = false,
        }, cancellationToken);

        var keyboard = new InlineKeyboardMarkup(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Submit], CallbackData.Submit),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Cancel], CallbackData.Cancel)],
        ]);

        var text = localizer[
            Text.TransactionConfirmation,
            customer.Card.Ballance,
            price,
            user.GetFullName(),
            DateTimeOffset.Now.ToString("dd.MM.yyyy")];

        try
        {
            await botClient.DeleteMessageAsync(
                chatId: customer.TelegramId,
                messageId: customer.MessageId,
                cancellationToken: cancellationToken);
        }
        catch { }

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: customer.ChatId,
            text: text,
            replyMarkup: keyboard,
            //parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);

        customer.MessageId = sentMessage.MessageId;
        customer.PlaceId = user.Id;
        customer.State = States.WaitingForConfirmation;
    }

    private async Task HandleConfirmationAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        var transaction = await appDbContext.Transactions
            .OrderBy(t => t.Id)
            .LastAsync(t
            => t.SellerId.Equals(user.PlaceId) && t.CustomerId.Equals(user.Id),
            cancellationToken);

        switch (callbackQuery.Data)
        {
            case CallbackData.Submit:
                transaction.Status = Status.Completed;
                user.Card.Ballance -= transaction.Amount;
                await SendCustomerMenuAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.TransactionSucceeded]);
                break;
            case CallbackData.Cancel:
                transaction.Status = Status.Cancelled;
                await SendCustomerMenuAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.TransactionCanceled]);
                break;
            default: break;
        }
    }


    #region Is ready
    private bool IsCOntactValid(Domain.Entities.Contact contact) =>
        contact is not null &&
        !string.IsNullOrWhiteSpace(contact.Phone) &&
        !string.IsNullOrWhiteSpace(contact.Phone);

    public bool IsAccountComplete(Domain.Entities.User user)
        => !string.IsNullOrWhiteSpace(user.FirstName) &&
           !string.IsNullOrWhiteSpace(user.LastName) &&
           !string.IsNullOrWhiteSpace(user.Contact.Email) &&
           !string.IsNullOrWhiteSpace(user.Contact.Phone) &&
           user.Gender != Genders.Unknown &&
           user.DateOfBirth != DateTimeOffset.MinValue &&
           user.DateOfBirth.Year > 0;

    private static bool IsStoreReady(Store? store)
        => store is not null
        && store.Description is not null
        && store.Name is not null;

    private static bool IsAddressValid(Address? address) =>
        address is not null &&
        address.Latitude != default &&
        address.Longitude != default &&
        !string.IsNullOrEmpty(address?.Country) &&
        !string.IsNullOrEmpty(address?.Region) &&
        !string.IsNullOrEmpty(address?.District) &&
        !string.IsNullOrEmpty(address?.City) &&
        !string.IsNullOrEmpty(address?.Street) &&
        !string.IsNullOrEmpty(address?.HouseNumber);

    private async Task SendNotReadyNotificationAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);

        var sentMessage = await EditOrSendMessageAsync(
            botClient,
            message,
            localizer[Text.NotReadyNotification],
            cancellationToken);

        user.MessageId = sentMessage.MessageId;
    }
    #endregion
}
