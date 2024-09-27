namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task SendGreetingAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Seller], CallbackData.Seller)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Customer], CallbackData.Customer)]
        });

        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);

        await botClient.EditMessageTextAsync(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            text: localizer[Text.Greeting, user.FirstName, user.LastName],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        user.MessageId = message.MessageId;
        user.State = States.WaitingForSelectRole;
    }

    private async Task HandleSelectedUserRoleAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Data);
        ArgumentNullException.ThrowIfNull(callbackQuery.Message);

        user.Role = callbackQuery.Data switch
        {
            CallbackData.Seller => Roles.Seller,
            CallbackData.Customer => Roles.Customer,
            _ => Roles.None
        };

        await (user.Role switch
        {
            Roles.Seller => SendRequestForMarketNameAsync(botClient, callbackQuery.Message, cancellationToken),
            Roles.Customer => SendRequestForPhoneNumberAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }
}
