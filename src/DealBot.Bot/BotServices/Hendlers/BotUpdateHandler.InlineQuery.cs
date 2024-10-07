namespace DealBot.Bot.BotServices;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

public partial class BotUpdateHandler
{
    private async Task HandleInlineQueryAsync(ITelegramBotClient botClient, InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        var results = new InlineQueryResult[]
        {
        new InlineQueryResultArticle(
            id: "1",
            title: "Hello World",
            inputMessageContent: new InputTextMessageContent("Hello from inline query!"))
        };

        await botClient.AnswerInlineQueryAsync(
            inlineQueryId: inlineQuery.Id,
            results: results,
            isPersonal: true,
            cacheTime: 0,
            cancellationToken: cancellationToken
        );
    }
}
