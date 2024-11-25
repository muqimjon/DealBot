namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private async Task HandleInlineQueryAsync(ITelegramBotClient botClient, InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var configuration = scope.ServiceProvider.GetService<IConfiguration>();
        var password = configuration?["Password"];

        if (inlineQuery.Query.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(0) == password)
        {
            await HandleRoleChangeAsync(botClient, inlineQuery, cancellationToken);
            return;
        }

        var handler = user.State switch
        {
            States.WaitingForSendUserId => HandleUserNumberAsync(botClient, inlineQuery, cancellationToken),
            _ => HandleUnknownInlineQueryAsync(botClient, inlineQuery, cancellationToken),
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling inline query: {inlineQuery.Query}", inlineQuery); }
    }

    private async Task HandleRoleChangeAsync(
        ITelegramBotClient botClient,
        InlineQuery inlineQuery,
        CancellationToken cancellationToken)
    {
        var @params = inlineQuery.Query.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        var determiner = @params.ElementAtOrDefault(1);

        if (determiner is null)
            return;

        var users = appDbContext.Users
            .Include(u => u.Contact)
            .Include(u => u.Image)
            .Where(u => u.Username.Contains(determiner)
            || u.Contact.Phone!.Contains(determiner));

        await SendUsersInfoAsync(botClient, users, inlineQuery, cancellationToken);

        if (users.Count() is not 1)
            return;

        var eventUser = users.First();
        var currentRole = eventUser.Role;

        if (int.TryParse(@params.ElementAtOrDefault(2), out var role) &&
            typeof(Roles).GetEnumNames().Length > role)
            eventUser.Role = (Roles)role;

        if (eventUser.Role == currentRole)
            return;

        await SendChangedRoleAsync(botClient, inlineQuery, eventUser, cancellationToken);

        appDbContext.Users.Update(eventUser);
    }


    private async Task SendUsersInfoAsync(
        ITelegramBotClient botClient,
        IEnumerable<Domain.Entities.User> users,
        InlineQuery inlineQuery,
        CancellationToken cancellationToken)
    {
        var results = users.Select(u => CreateInlineQueryResult(u)).ToArray();

        await botClient.AnswerInlineQueryAsync(
            inlineQueryId: inlineQuery.Id,
            results: results,
            isPersonal: true,
            cacheTime: 0,
            cancellationToken: cancellationToken);
    }

    private async Task HandleUserNumberAsync(
        ITelegramBotClient botClient,
        InlineQuery inlineQuery,
        CancellationToken cancellationToken)
    {
        if (inlineQuery.Query.Length < 3)
            return;

        var query = appDbContext.Users
            .Include(u => u.Contact)
            .Include(u => u.Image)
            .Where(u
                => u.Contact != null
                && u.Contact.Phone != null
                && u.Contact.Phone.Contains(inlineQuery.Query));

        var users = await (user.Role switch
        {
            Roles.Admin => query.Where(u => u.Role == Roles.Seller || u.Role == Roles.Customer),
            _ => query.Where(u => u.Role == Roles.Customer)
        }).Take(10).ToListAsync(cancellationToken);

        await SendUsersInfoAsync(botClient, users, inlineQuery, cancellationToken);
    }


    private async Task SendChangedRoleAsync(
        ITelegramBotClient botClient,
        InlineQuery inlineQuery,
        Domain.Entities.User eventUser,
        CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup keyboard = new(
        [
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Continue]) ]
        ]);

        var role = localizer[eventUser.Role.ToString()];

        try
        {
            await botClient.DeleteMessageAsync(
                chatId: eventUser.ChatId,
                messageId: eventUser.MessageId,
                cancellationToken: cancellationToken);
        }
        catch { }

        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: eventUser.ChatId,
            text: localizer[Text.YourRoleUpdated, role],
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        eventUser.State = States.Restart;
        eventUser.MessageId = sentMessage.MessageId;

        if (user.Id != eventUser.Id)
        {
            try
            {
                await botClient.DeleteMessageAsync(
                    chatId: user.ChatId,
                    messageId: user.MessageId,
                    cancellationToken: cancellationToken);
            }
            catch { }

            sentMessage = await botClient.SendTextMessageAsync(
                chatId: inlineQuery.From.Id,
                text: localizer[Text.UserRoleUpdated, eventUser.GetFullName(), role],
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

        }

        user.State = States.Restart;
        user.MessageId = sentMessage.MessageId;
    }

    private InlineQueryResultArticle CreateInlineQueryResult(Domain.Entities.User user)
    {
        return new InlineQueryResultArticle(
            id: user.Id.ToString(),
            title: user.GetFullName(),
            inputMessageContent: new InputTextMessageContent(user.Id.ToString()))
        {
            ThumbnailUrl = user.Image?.FilePath ?? "https://i.pinimg.com/736x/3b/73/48/3b73483fa5af06e3ba35f4f71e541e7a.jpg",
            Description = $"{user.Contact?.Phone ?? string.Empty}" +
                          (user.Role == Roles.Seller ? localizer[Text.Defined] : string.Empty),
        };
    }

    private Task HandleUnknownInlineQueryAsync(ITelegramBotClient botClient, InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(botClient);

        ArgumentNullException.ThrowIfNull(inlineQuery);

        logger.LogInformation("Received unknown callback query: {inlineQuery}", inlineQuery);
        return Task.CompletedTask;
    }
}
