namespace DealBot.Bot.BotServices;

using DealBot.Application.Common;
using DealBot.Application.UseCases.Users.Queries;
using DealBot.Application.Users.Commands.CreateUser;
using DealBot.Bot.Resources;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class BotUpdateHandler(
    ILogger<BotUpdateHandler> logger,
    IAppDbContext context,
    IServiceScopeFactory serviceScopeFactory)
    : IUpdateHandler
{
    private IStringLocalizer<BotLocalizer> localizer = default!;
    private ISender sender = default!;
    private User user = default!;

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<BotLocalizer>>();
        sender = scope.ServiceProvider.GetRequiredService<ISender>();

        user = await context.Users.FirstOrDefaultAsync(i => i.TelegramId.Equals(update.Message.From.Id))
        SetCulture(user.LanguageCode);

        var handlerTask = update.Type switch
        {
            UpdateType.Message => HandleMessageAsync(botClient, update.Message!, cancellationToken),
            UpdateType.CallbackQuery => HandleCallbackQueryAsync(botClient, update.CallbackQuery!, cancellationToken),
            _ => HandleUnknownUpdateAsync(botClient, update, cancellationToken)
        };

        user.Address.Street = "sdfsdf";

        try
        {
            await handlerTask;
            await sender.Send(MapTo<UserResultDto, UpdateUserCommand>(user), cancellationToken);
        }
        catch (Exception ex) { await HandlePollingErrorAsync(botClient, ex, cancellationToken); }
    }

    private async Task<UserResultDto> GetUserAsync(Update update, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(update.Message);
        ArgumentNullException.ThrowIfNull(update.Message.From);

        var updateContent = GetUpdateContent(update);
        var from = updateContent.From!;

        return await sender.Send(new GetUserByTelegramIdQuery(from.Id), cancellationToken)
            ?? await sender.Send(new CreateUserCommand
            {
                IsBot = from.IsBot,
                TelegramId = from.Id,
                LastName = from.LastName,
                Username = from.Username,
                FirstName = from.FirstName,
                IsPremium = from.IsPremium,
                ChatId = update.Message?.Chat.Id,
                LanguageCode = from.LanguageCode,
            }, cancellationToken);
    }

    private static dynamic GetUpdateContent(Update update) => update.Type switch
    {
        UpdateType.Message => update.Message!,
        UpdateType.ChatMember => update.ChatMember!,
        UpdateType.CallbackQuery => update.CallbackQuery!,
        _ => update.Message!,
    };

    private static void SetCulture(string? languageCode)
    {
        var culture = languageCode switch
        {
            "uz" => new CultureInfo("uz-UZ"),
            "en" => new CultureInfo("en-US"),
            "ru" => new CultureInfo("ru-RU"),
            _ => CultureInfo.CurrentCulture
        };

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public static TDestination MapTo<TSource, TDestination>(TSource source)
        where TDestination : class, new()
    {
        ArgumentNullException.ThrowIfNull(source);

        var propSource = typeof(TSource).GetProperties();
        var propDestination = typeof(TDestination).GetProperties();
        var destination = new TDestination();

        foreach (var ps in propSource)
        {
            var pd = propDestination.FirstOrDefault(p
                => p.Name.Equals(ps.Name) && p.CanWrite);

            if (pd != null && pd.PropertyType == ps.PropertyType)
            {
                var value = ps.GetValue(source);
                pd.SetValue(destination, value);
            }
        }

        return destination;
    }

    private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling message: {MessageText}", message.Text);

        await botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing, cancellationToken: cancellationToken);

        await Task.Delay(1000, cancellationToken);

        await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Salom",
                cancellationToken: cancellationToken);
    }

    private Task HandleCallbackQueryAsync(ITelegramBotClient _, CallbackQuery callbackQuery, CancellationToken __)
    {
        logger.LogInformation("Handling callback query: {CallbackData}", callbackQuery.Data);
        // Add your callback handling logic here
        return Task.CompletedTask;
    }

    private Task HandleUnknownUpdateAsync(ITelegramBotClient _, Update update, CancellationToken __)
    {
        logger.LogWarning("Received unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error occurred while polling: {ErrorMessage}", exception.Message);
        return Task.CompletedTask;
    }
}
