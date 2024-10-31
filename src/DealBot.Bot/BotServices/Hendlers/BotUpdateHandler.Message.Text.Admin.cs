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

        var store = await (from s in appDbContext.Stores
                           orderby s.CreatedAt descending
                           select s).FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskName, store.Name],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendName;
    }

    private async Task HandleNameAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        // TO DO need Validation
        var store = await appDbContext.Stores.FirstAsync(cancellationToken);
        store.Name = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.NameUpdated]);
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

        var store = await (from s in appDbContext.Stores
                           orderby s.CreatedAt descending
                           select s).FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskDescription, store.Description],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendDescription;
    }

    private async Task HandleDesctiptionAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        // TO DO need Validation
        var store = await appDbContext.Stores.FirstAsync(cancellationToken);
        store.Description = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.DescriptionUpdated]);
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

        var store = await (from s in appDbContext.Stores
                           orderby s.CreatedAt descending
                           select s).FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskMiniAppUrl, store.Description],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendMiniAppUrl;
    }

    private async Task HandleMiniAppUrlAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        // TO DO need Validation
        var store = await appDbContext.Stores.FirstAsync(cancellationToken);
        store.Name = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.MiniAppUrlUpdated]);
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

        var store = await (from s in appDbContext.Stores
                           orderby s.CreatedAt descending
                           select s).FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskWebsite, store.Description],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendWebsite;
    }

    private async Task HandleWebsiteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        // TO DO need Validation
        var store = await appDbContext.Stores.FirstAsync(cancellationToken);
        store.Website = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.WebsiteUpdated]);
    }

    private async Task SendRequestForPhoneNumberAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        ReplyKeyboardMarkup keyboard = new(new KeyboardButton[][]
        {
            [new(localizer[Text.SendContact]){ RequestContact = true}],
            [new(localizer[Text.Back])]
        })
        {
            ResizeKeyboard = true,
            InputFieldPlaceholder = localizer[Text.AskPhoneNumberInPlaceHolder],
        };

        var store = await (
            from s in appDbContext.Stores
            orderby s.CreatedAt descending
            select s)
            .Include(s => s.Contact)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        store.Contact ??= new() { Phone = Text.Undefined };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskPhoneNumber, store.Contact.Phone!],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendCompanyPhoneNumber;
    }

    private async Task HandleCompanyPhoneNumberAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        // TO DO need Validation
        var store = await appDbContext.Stores
            .Include(s => s.Contact)
            .FirstAsync(cancellationToken);

        store.Contact.Phone = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.PhoneNumberUpdated]);
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

        var store = await (from s in appDbContext.Stores
                           orderby s.CreatedAt descending
                           select s)
                           .Include(s => s.Contact)
                           .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        store.Contact ??= new() { Email = Text.Undefined };

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskEmail, store.Contact.Email!],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendCompanyEmail;
    }

    private async Task HandleCompanyEmailAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        // TO DO need Validation
        var store = await appDbContext.Stores
            .Include(s => s.Contact)
            .FirstAsync(cancellationToken);

        store.Contact.Email = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.EmailUpdated]);
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

        var store = await (from s in appDbContext.Stores
                           orderby s.CreatedAt descending
                           select s)
                           .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.Error]);
            return;
        }

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: localizer[Text.AskChannel, store.Channel],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        await botClient.DeleteMessageAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSendChannel;
    }

    private async Task HandleChannelAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message.Text, nameof(message));

        // TO DO need Validation
        var store = await appDbContext.Stores
            .FirstAsync(cancellationToken);

        store.Channel = message.Text;

        await SendMenuCompanyInfoAsync(botClient, message, cancellationToken, localizer[Text.ChannelUpdated]);
    }
}
