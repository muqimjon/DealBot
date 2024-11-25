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
    private async Task CheckMissionTransactionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var transaction = await appDbContext.Transactions
            .Include(t => t.Seller)
            .Include(t => t.Customer)
            .OrderBy(t => t.Id)
            .LastOrDefaultAsync(t
                => t.CustomerId.Equals(user.PlaceId)
                && t.Status.Equals(Status.None),
            cancellationToken: cancellationToken);

        Domain.Entities.User customer = transaction?.Customer
            ?? await appDbContext.Users.FirstAsync(c => c.Id.Equals(user.PlaceId), cancellationToken: cancellationToken);

        if (!await IsSubscribed(botClient, customer.TelegramId, cancellationToken))
        {
            await SendRequestJoinToChannel(botClient, message, cancellationToken, localizer[Text.RequiredToSubscribe], customer);
            await SendUserManagerMenuAsync(botClient, message, cancellationToken, localizer[Text.RequiredToSubscribe]);
        }
        else if (transaction is not null)
            await SendMissingTransactionAsync(transaction, botClient, message, cancellationToken);
        else
            await SendTransactionAsync(botClient, message, cancellationToken);
    }

    private async Task SendTransactionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Take], CallbackData.Take),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Give], CallbackData.Give)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        ]);

        var customer = await appDbContext.Users
            .Include(c => c.Card)
            .FirstAsync(c => c.Id.Equals(user.PlaceId), cancellationToken);

        var text = string.Concat(actionMessage,
            localizer[
                Text.CustomerCardInfo,
                customer.GetFullName(),
                customer.Card.Ballance,
                localizer[customer.Card.Type.ToString()],
                customer.IsActive ? Text.Active : Text.Inactive,
                customer.Card.State]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectTransaction;
    }

    private async Task HandleSelectedCashbackTransferAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.Take => SendProductPriceInquiryAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Give => SendSalesAmountInquiryAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => default!
        });
    }

    #region Cashback transfer

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

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Simple], CallbackData.Simple)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Premium], CallbackData.Premium)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

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
            keyboard: keyboard,
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

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData("<<", CallbackData.Previous2),
                InlineKeyboardButton.WithCallbackData("<", CallbackData.Previous),
                InlineKeyboardButton.WithCallbackData(">", CallbackData.Next),
                InlineKeyboardButton.WithCallbackData(">>", CallbackData.Next2)],
                [InlineKeyboardButton.WithCallbackData(localizer[Text.Submit], CallbackData.Submit)],
                [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

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

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData("<<", CallbackData.Previous2),
                InlineKeyboardButton.WithCallbackData("<", CallbackData.Previous),
                InlineKeyboardButton.WithCallbackData(">", CallbackData.Next),
                InlineKeyboardButton.WithCallbackData(">>", CallbackData.Next2)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Cancel], CallbackData.Cancel),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Submit], CallbackData.Submit)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

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

    private async Task SendMissingTransactionAsync(Transaction transaction, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Remind], CallbackData.Remind),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Cancel], CallbackData.Cancel)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Check], CallbackData.Check)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

        var text = string.Concat(actionMessage, localizer
            [
                Text.TransactionInfo,
                transaction.Amount,
                transaction.Status,
                transaction.Customer.GetFullName(),
                transaction.Seller.GetFullName()
            ]);

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectTransaction;
    }

    private async Task HandleSelectedTransactionMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.Remind => SendRemindTransactionAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Cancel => CancelTransactionAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Check => CheckMissionTransactionAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Take => SendProductPriceInquiryAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => default!
        });
    }

    private async Task CancelTransactionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var transaction = await appDbContext.Transactions
            .OrderBy(t => t.Id)
            .LastOrDefaultAsync(t
                => t.CustomerId.Equals(user.PlaceId)
                && t.Status.Equals(Status.None),
            cancellationToken: cancellationToken);

        if (transaction is null)
            return;

        transaction.Status = Status.Cancelled;
        await SendTransactionAsync(botClient, message, cancellationToken);
    }

    private async Task SendRemindTransactionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var transaction = await appDbContext.Transactions
                .Include(t => t.Customer)
                .OrderBy(t => t.Id)
                .LastOrDefaultAsync(t
                    => t.CustomerId.Equals(user.PlaceId)
                    && t.Status.Equals(Status.Pending),
                cancellationToken: cancellationToken);

        if (transaction is null)
            return;

        transaction.Seller = user;
        transaction.SellerId = user.Id;

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Submit], CallbackData.Submit),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Cancel], CallbackData.Cancel)],
        ]);

        await botClient.SendTextMessageAsync(
            chatId: transaction.Customer.TelegramId,
            text: localizer[Text.RemindMessage],
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await SendMissingTransactionAsync(transaction, botClient, message, cancellationToken, localizer[Text.MessageSent]);
    }
    #endregion

    #region Send message
    private async Task HandleSendMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var value = (await appDbContext.MyMessages
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync(m => m.Status == Status.Pending, cancellationToken))!;

        if (value is null)
            await SendRequestMessageTextAsync(botClient, message, cancellationToken);
        else
            await SendMessageMenuAsync(botClient, message, cancellationToken, myMessage: value);
    }

    private async Task SendMessageMenuAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty, MyMessage myMessage = default!)
    {
        await RedirectToAskMessageTextAsync(botClient, message, myMessage, cancellationToken);

        InlineKeyboardMarkup keyboard = new(
        [
                IsMessageToAll(myMessage) ? [] : [InlineKeyboardButton.WithCallbackData(localizer[Text.ToAll], CallbackData.ToAll)],
                [InlineKeyboardButton.WithCallbackData(localizer[Text.Gender], CallbackData.Gender),
                    InlineKeyboardButton.WithCallbackData(localizer[Text.PhoneNumber], CallbackData.PhoneNumber)],
                [InlineKeyboardButton.WithCallbackData(localizer[Text.Cancel], CallbackData.Cancel),
                    InlineKeyboardButton.WithCallbackData(localizer[Text.Submit], CallbackData.Submit)],
                [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        ]);

        var text = string.Concat(actionMessage, localizer[Text.SelectMenu, myMessage!.Content]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            keyboard: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForMessageMenu;
    }

    private async Task RedirectToAskMessageTextAsync(ITelegramBotClient botClient, Message message, MyMessage myMessage, CancellationToken cancellationToken)
    {
        if (myMessage is null)
        {
            var actionMessage = localizer[Text.Error];

            await (user.Role switch
            {
                Roles.Admin => SendAdminMenuAsync(botClient, message, cancellationToken, actionMessage),
                Roles.Seller => SendSellerMenuAsync(botClient, message, cancellationToken, actionMessage),
                _ => SendCustomerMenuAsync(botClient, message, cancellationToken, actionMessage)
            });

            return;
        }
    }

    private async Task HandleSelectedMessageMenuAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        await (callbackQuery.Data switch
        {
            CallbackData.ToAll => HandleToAllAsync(botClient, callbackQuery.Message, cancellationToken), // TO DO
            CallbackData.Gender => SendRequestForGender(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.PhoneNumber => SendRequestForUserIdAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Cancel => SendRequestForUserIdAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Submit => SendRequestForUserIdAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => default!
        });
    }

    private static bool IsMessageToAll(MyMessage myMessage)
        => myMessage.CardType == CardTypes.None
                    && myMessage.Recipient is null
                    && myMessage.Gender == Genders.Unknown
                    && myMessage.Role == Roles.None;


    private async Task SendRequestForGender(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        var prepareMessage = await appDbContext.MyMessages
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync(m => m.Status == Status.Pending, cancellationToken);

        if (prepareMessage is null)
            await appDbContext.MyMessages.AddAsync(prepareMessage = new() { Status = Status.Pending });

        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Male], CallbackData.Male),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Female], CallbackData.Female)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        ]);

        var text = string.Concat(actionMessage, localizer[Text.AskGender, localizer[prepareMessage.Gender.ToString()]]);
        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectGenderForMessage;
    }

    private async Task HandleSelectedGenderForMessageAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message);

        var value = user;
        if (user.PlaceId != 0 && (value = await appDbContext.Users.FirstOrDefaultAsync(u => u.Id == user.PlaceId, cancellationToken)) is null)
        {
            await SendAdminMenuAsync(botClient, callbackQuery.Message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var actionMessage = string.Concat(
            localizer[Text.Empty]);

        value.Gender = callbackQuery.Data switch
        {
            CallbackData.Female => Genders.Female,
            CallbackData.Male => Genders.Male,
            _ => Genders.Unknown,
        };

        await SendMessageMenuAsync(botClient, callbackQuery.Message, cancellationToken, actionMessage);
    }

    private Task HandleToAllAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    #endregion
}
