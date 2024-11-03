namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
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

        var query = appDbContext.Users
            .Include(user => user.Contact)
            .Include(user => user.Image)
            .Where(u
                => u.Contact != null
                && u.Contact.Phone != null
                && u.Contact.Phone.Contains(inlineQuery.Query));

        var users = (user.Role switch
        {
            Roles.Admin => query.Where(u =>  u.Role == Roles.Seller || u.Role == Roles.Customer),
            _ => query.Where(u => u.Role == Roles.Customer)
        }).Take(10).ToList();

        var results = users.Select(user => new InlineQueryResultArticle(
            id: user.Id.ToString(),
            title: user.GetFullName(),
            inputMessageContent: new InputTextMessageContent
            (user.Id.ToString()))
        {
            ThumbnailUrl = user.Image?.FilePath ?? "https://i.pinimg.com/736x/3b/73/48/3b73483fa5af06e3ba35f4f71e541e7a.jpg",
            Description = user.Contact.Phone + (user.Role == Roles.Seller ? localizer[Text.Defined] : Text.Empty),
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
