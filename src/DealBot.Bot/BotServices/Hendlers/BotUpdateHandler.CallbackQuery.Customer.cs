namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendCustomerMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.MyPrivilegeCard], CallbackData.MyPrivilegeCard)],
            [InlineKeyboardButton.WithWebApp(localizer[Text.OrderOnTheSite], new WebAppInfo() { Url = "https://idea-up.uz"})],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Settings], CallbackData.Settings),
                InlineKeyboardButton.WithCallbackData(localizer[Text.StoreAddress], CallbackData.StoreAddress)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.ContactUs], CallbackData.ContactUs),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Comment], CallbackData.Comment)],
            [InlineKeyboardButton.WithUrl(localizer[Text.Referral], await GetShareLink(botClient, cancellationToken))],
        });

        user.Card.State = await IsSubscribed(botClient, user.TelegramId, cancellationToken)
            ? CardStates.Active : CardStates.Block;
        user.IsActive = IsAccountComplete(user);

        var text = string.Concat(actionMessage,
            localizer[Text.UserInfo, user.GetFullName(), user.Card.Ballance, user.Card.Type],
            localizer[Text.SelectMenu]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            replyMarkup: keyboard,
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
            CallbackData.StoreAddress => SendStoreAddressAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.ContactUs => SendContactInfoAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Comment => SendRequestCommentAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Settings => SendCustomerSettingsAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task SendRequestJoinToChannel(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty, Domain.Entities.User customer = default!)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithUrl(localizer[Text.Subscribe], "https://t.me/milestonies")],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Check], CallbackData.Check)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

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
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

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
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: localizer[Text.StoreAddressInfo],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectAddressOption;
    }

    private async Task SendContactInfoAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: localizer[Text.StoreContactInfo],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectStoreContactOption;
    }

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

    private async Task<bool> IsSubscribed(ITelegramBotClient botClient, long userId, CancellationToken cancellationToken)
    {
        var member = await botClient.GetChatMemberAsync(
            chatId: "@Milestonies",
            userId,
            cancellationToken: cancellationToken);

        return (int)member.Status < 4;
    }
    public bool IsAccountComplete(Domain.Entities.User user)
    {
        return !string.IsNullOrWhiteSpace(user.FirstName) &&
               !string.IsNullOrWhiteSpace(user.LastName) &&
               !string.IsNullOrWhiteSpace(user.Contact.Email) &&
               !string.IsNullOrWhiteSpace(user.Contact.Phone) &&
               !user.Gender.Equals(Genders.Unknown) &&
               user.DateOfBirth != DateTime.MinValue;
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
            Status = CashBackStatus.Pending,
            Customer = customer,
            Seller = user,
            IsCashback = false,
        }, cancellationToken);

        var keyboard = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Submit], CallbackData.Submit),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Cancel], CallbackData.Cancel)],
        });

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
                transaction.Status = CashBackStatus.Completed;
                user.Card.Ballance -= transaction.Amount;
                await SendCustomerMenuAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.TransactionSucceeded]);
                break;
            case CallbackData.Cancel:
                transaction.Status = CashBackStatus.Cancelled;
                await SendCustomerMenuAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.TransactionCanceled]);
                break;
            default: break;
        }
    }
}
