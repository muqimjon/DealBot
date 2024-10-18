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
                && t.Status.Equals(CashBackStatus.None),
            cancellationToken: cancellationToken);

        if (transaction is null)
            await SendTransactionAsync(botClient, message, cancellationToken);
        else
            await SendMissingTransactionAsync(transaction, botClient, message, cancellationToken);
    }

    private async Task SendTransactionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Give], CallbackData.Take),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Get], CallbackData.Give)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]
        });
        var customer = await appDbContext.Users
            .Include(c => c.Card)
            .FirstAsync(c => c.Id.Equals(user.PlaceId), cancellationToken);
        var text = localizer[Text.CustomerCardInfo, customer.GetFullName(), customer.Card.Ballance, customer.Card.Type];

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: text,
            replyMarkup: keyboard,
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
    private async Task SendMissingTransactionAsync(Transaction transaction, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Remind], CallbackData.Remind),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Cancel], CallbackData.Cancel)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Check], CallbackData.Check)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

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
                && t.Status.Equals(CashBackStatus.None),
            cancellationToken: cancellationToken);

        if (transaction is null)
            return;

        transaction.Status = CashBackStatus.Cancelled;
        await SendTransactionAsync(botClient, message, cancellationToken);
    }

    private async Task SendRemindTransactionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var transaction = await appDbContext.Transactions
                .Include(t => t.Customer)
                .OrderBy(t => t.Id)
                .LastOrDefaultAsync(t
                    => t.CustomerId.Equals(user.PlaceId)
                    && t.Status.Equals(CashBackStatus.Pending),
                cancellationToken: cancellationToken);

        if (transaction is null)
            return;

        transaction.Seller = user;
        transaction.SellerId = user.Id;

        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Submit], CallbackData.Submit),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Cancel], CallbackData.Cancel)],
        });

        await botClient.SendTextMessageAsync(
            chatId: transaction.Customer.TelegramId,
            text: localizer[Text.RemindMessage],
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await SendMissingTransactionAsync(transaction, botClient, message, cancellationToken, localizer[Text.MessageSent]);
    }
    #endregion
}
