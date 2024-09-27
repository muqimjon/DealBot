namespace DealBot.Bot.BotServices;

using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

public partial class BotUpdateHandler
{
    private async Task SendRequestForMarketNameAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        await botClient.SendChatActionAsync(
            chatId: message.Chat.Id,
            chatAction: ChatAction.Typing,
            cancellationToken: cancellationToken);
    }
}
