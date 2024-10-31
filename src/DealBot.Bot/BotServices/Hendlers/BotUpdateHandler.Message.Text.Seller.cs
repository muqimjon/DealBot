﻿namespace DealBot.Bot.BotServices;

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
    private async Task SendRequestForUserIdAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        await botClient.SendChatActionAsync(
            chatAction: ChatAction.Typing,
            chatId: message.Chat.Id,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskUserIdInPlaceHolder,
            (await botClient.GetMeAsync(cancellationToken)).Username!],
        };

        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskUserId],
            replyMarkup: keyboard,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);

        var bot = await botClient.GetMeAsync(cancellationToken: cancellationToken);
        bot.SupportsInlineQueries = true;

        user.State = States.WaitingForSendUserId;
        user.MessageId = sentMessage.MessageId;
    }

    private async Task HandleUserIdAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var bot = await botClient.GetMeAsync(cancellationToken: cancellationToken);
        bot.SupportsInlineQueries = false;
        Domain.Entities.User customer = default!;

        if (message.ViaBot is not null && message.ViaBot.Id.Equals(bot.Id) && long.TryParse(message.Text, out var id))
            customer = (await appDbContext.Users.FirstOrDefaultAsync(user
                    => user.Id.Equals(id),
                cancellationToken: cancellationToken))!;

        if (customer is null)
        {
            await SendSellerMenuAsync(botClient, message, cancellationToken);
            return;
        }

        user.PlaceId = customer.Id;
        await SendUserManagerMenuAsync(botClient, message, cancellationToken);
    }

    private async Task SendProductPriceInquiryAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
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

        var text = string.Concat(actionMessage, localizer[Text.AskProductPrice]);

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: text,
            replyMarkup: keyboard,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendProductPrice;
    }

    private async Task HandleProductPriceAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(message.Text, nameof(message));

        if (decimal.TryParse(message.Text.Trim(), out var price) && price > 0)
        {
            var customer = await appDbContext.Users
                .Include(u => u.Card)
                .FirstAsync(u => u.Id.Equals(user.PlaceId), cancellationToken);

            if (price > customer.Card.Ballance)
            {
                await SendTransactionAsync(botClient, message, cancellationToken, localizer[Text.LackOfBalance]);
                return;
            }

            await SendCustomerConfirmation(botClient, price, customer, cancellationToken);
            await SendUserManagerMenuAsync(botClient, message, cancellationToken, localizer[Text.SentConfirmation]);
        }
        else
            await SendProductPriceInquiryAsync(botClient, message, cancellationToken, localizer[Text.AskCorrectNumber]);
    }

    private async Task SendRequestCustomerConfirmationAsync(Transaction transaction, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string actionMessage = Text.Empty)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Submit], CallbackData.Submit),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Cancel], CallbackData.Cancel)],
        });

        var text = string.Concat(actionMessage, localizer[Text.Confirmation]);

        var sentMessage = await EditOrSendMessageAsync(
            botClient: botClient,
            message: message,
            text: text,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken,
            messageId: transaction.Customer.MessageId);

        transaction.Customer.MessageId = sentMessage.MessageId;
        transaction.Customer.State = States.WaitingForConfirmation;
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
            text: localizer[Text.SalesAmount],
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
            var customer = await appDbContext.Users
                .Include(c => c.Card)
                .FirstAsync(c => c.Id.Equals(user.PlaceId), cancellationToken);

            customer.Card ??= new();
            customer.Card.Ballance += price * customer.Card.Type switch
            {
                CardTypes.Simple => 0.02m,
                CardTypes.Premium => 0.4m,
                _ => 0,
            };

            var transaction = await appDbContext.Transactions.AddAsync(new()
            {
                Amount = price,
                Customer = customer,
                IsCashback = true,
                Seller = user,
                Status = CashBackStatus.Completed,
                CustomerId = customer.Id,
                SellerId = user.Id,
            });
        }

        var text = localizer[Text.TransactionSucceeded];

        await SendUserManagerMenuAsync(botClient, message, cancellationToken, text);
    }

    private async Task SendRequestMessageToDeveloperAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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

        await SendSellerSettingsAsync(botClient, message, cancellationToken, localizer[Text.MessageSent]);
    }
}
