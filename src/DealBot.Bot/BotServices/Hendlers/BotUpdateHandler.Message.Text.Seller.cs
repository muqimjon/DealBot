namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendProductPriceInquiryAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskProductPriceInPlaceHolder],
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskProductPrice],
            replyMarkup: keyboard,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendProductPrice;
    }

    private async Task HandleProductPriceAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var customer = (await appDbContext.Users
            .Include(u => u.Card)
            .FirstOrDefaultAsync(u => u.Id.Equals(user.PlaceId), cancellationToken))!;

        if (decimal.TryParse(message.Text, out var price) && price <= customer.Card.Ballance)
        {
            var transaction = await appDbContext.Transactions
                .Include(t => t.Seller)
                .Include(t => t.Customer)
                .LastOrDefaultAsync(t
                    => t.CustomerId.Equals(user.PlaceId)
                    && t.Status.Equals(CashBackStatus.None),
                cancellationToken: cancellationToken);

            if (transaction is not null)
            {
                await SendMissingTransactionAsync(transaction, botClient, message, cancellationToken);
                return;
            }
        }
        else
            await SendUserManagerMenuAsync(botClient, message, cancellationToken);
    }

    private async Task SendSalesAmountInquiryAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskSalesAmountInPlaceHolder],
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskProductPrice],
            replyMarkup: keyboard,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendSalesAmount;
    }

    private async Task HandleSalesAmountAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (decimal.TryParse(message.Text, out var price))
        {
            var customer = (await appDbContext.Users.FindAsync([user.PlaceId, cancellationToken], cancellationToken))!;
            customer.Card ??= new();
            customer.Card.Ballance += price * customer.Card.Type switch
            {
                CardType.Simple => 0.02m,
                CardType.Premium => 0.4m,
                _ => 0
            };

        }

        await SendRequestForUserIdAsync(botClient, message, cancellationToken);
    }


    private async Task SendRequestMessageForDeveloperAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskMessageInPlaceHolder],
        };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskMessage],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendMessageToDeveloper;
    }

    private async Task HandleMessageToDeveloperAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        await SendSellerSettingsAsync(botClient, message, cancellationToken);
    }
}
