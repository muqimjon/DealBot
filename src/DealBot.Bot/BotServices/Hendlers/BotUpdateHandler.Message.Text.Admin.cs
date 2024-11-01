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
    private async Task SendRequestForNameAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
            InputFieldPlaceholder = localizer[Text.AskNameInPlaceHolder],
        };

        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var name = string.IsNullOrEmpty(store.Name) ? Text.Undefined : store.Name;

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskName, name],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendName;
    }

    // TO DO need Validation
    private async Task HandleNameAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        var store = await appDbContext.Stores.FirstAsync(cancellationToken);
        var actionMessage = localizer[string.IsNullOrEmpty(store.Name) ? Text.SetSucceeded : Text.UpdateSucceeded];
        store.Name = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, actionMessage);
    }

    private async Task SendRequestForDescriptionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
            InputFieldPlaceholder = localizer[Text.AskDescriptionInPlaceHolder],
        };

        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var description = string.IsNullOrEmpty(store.Description) ? Text.Undefined : store.Description;

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskDescription, description],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendDescription;
    }

    // TO DO need Validation
    private async Task HandleDesctiptionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        var store = await appDbContext.Stores.FirstAsync(cancellationToken);
        var actionMessage = localizer[string.IsNullOrEmpty(store.Description) ? Text.SetSucceeded : Text.UpdateSucceeded];
        store.Description = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, actionMessage);
    }

    private async Task SendRequestForMiniAppUrlAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
            InputFieldPlaceholder = localizer[Text.AskMiniAppUrlInPlaceHolder],
        };

        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var miniAppUrl = string.IsNullOrEmpty(store.MiniAppUrl) ? Text.Undefined : store.MiniAppUrl;

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskMiniAppUrl, miniAppUrl],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendMiniAppUrl;
    }

    // TO DO need Validation
    private async Task HandleMiniAppUrlAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        var store = await appDbContext.Stores.FirstAsync(cancellationToken);
        var actionMessage = localizer[string.IsNullOrEmpty(store.Name) ? Text.SetSucceeded : Text.UpdateSucceeded];
        store.Name = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, actionMessage);
    }

    private async Task SendRequestForWebsiteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
            InputFieldPlaceholder = localizer[Text.AskWebsiteInPlaceHolder],
        };

        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var website = string.IsNullOrEmpty(store.Website) ? Text.Undefined : store.Website;

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskWebsite, website],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendWebsite;
    }

    // TO DO need Validation
    private async Task HandleWebsiteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        var store = await appDbContext.Stores.FirstAsync(cancellationToken);
        var actionMessage = localizer[string.IsNullOrEmpty(store.Website) ? Text.SetSucceeded : Text.UpdateSucceeded];
        store.Website = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, actionMessage);
    }

    private async Task SendRequestForEmailAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
            InputFieldPlaceholder = localizer[Text.AskEmailInPlaceHolder],
        };

        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .Include(s => s.Contact)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        store.Contact ??= new();
        var email = string.IsNullOrEmpty(store.Contact.Email) ? Text.Undefined : store.Contact.Email;

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskEmail, email],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendCompanyEmail;
    }

    // TO DO need Validation
    private async Task HandleCompanyEmailAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        var store = await appDbContext.Stores
            .Include(s => s.Contact)
            .FirstAsync(cancellationToken);

        var actionMessage = localizer[string.IsNullOrEmpty(store.Contact.Email) ? Text.SetSucceeded : Text.UpdateSucceeded];
        store.Contact.Email = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, actionMessage);
    }

    private async Task SendRequestForChannelAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
            InputFieldPlaceholder = localizer[Text.AskChannelInPlaceHolder],
        };

        var store = await appDbContext.Stores
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var channel = string.IsNullOrEmpty(store.Channel) ? Text.Undefined : store.Channel;

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskChannel, channel],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendChannel;
    }

    // TO DO need Validation
    private async Task HandleChannelAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        var store = await appDbContext.Stores
            .FirstAsync(cancellationToken);

        var actionMessage = localizer[string.IsNullOrEmpty(store.Channel) ? Text.SetSucceeded : Text.UpdateSucceeded];
        store.Channel = (message.Text.StartsWith('@') ? message.Text : '@' + message.Text).Trim();

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, actionMessage);
    }
}
