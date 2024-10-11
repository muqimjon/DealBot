namespace DealBot.Bot.BotServices;

using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

public partial class BotUpdateHandler
{
    private async Task HandleInlineQueryAsync(ITelegramBotClient botClient, InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        Task handler = user.State switch
        {
            States.WaitingForSendUserId => HandleUserNumberAsync(botClient, inlineQuery, cancellationToken),
            _ => HandleUnknownInlineQueryAsync(botClient, inlineQuery, cancellationToken),
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling inline query: {callbackQuery.Data}", inlineQuery); }
    }

    private async Task HandleUserNumberAsync(ITelegramBotClient botClient, InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        if (inlineQuery.Query.Length < 3)
            return;

        var users = appDbContext.Users
            .Where(user => user.Contact.Phone != null
                && user.Contact.Phone.Contains(inlineQuery.Query)
                && this.user.Role.Equals(Roles.Customer))
            .Include(user => user.Contact)
            .Include(user => user.Image)
            .Take(10)
            .ToList();

        var results = users.Select(user => new InlineQueryResultArticle(
            id: user.Id.ToString(),
            title: user.GetFullName(),
            inputMessageContent: new InputTextMessageContent
            (user.Id.ToString()))
        {
            ThumbnailUrl = user.Image?.FilePath ?? "https://i.pinimg.com/736x/3b/73/48/3b73483fa5af06e3ba35f4f71e541e7a.jpg",
            Description = user.Contact.Phone,
        }).ToArray();


        await botClient.AnswerInlineQueryAsync(
            inlineQueryId: inlineQuery.Id,
            results: results,
            isPersonal: true,
            cacheTime: 0,
            cancellationToken: cancellationToken);
    }

    private Task HandleUnknownInlineQueryAsync(ITelegramBotClient botClient, InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received unknown callback query: {inlineQuery}", inlineQuery);
        return Task.CompletedTask;
    }
}
