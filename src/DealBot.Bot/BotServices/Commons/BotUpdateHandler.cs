﻿namespace DealBot.Bot.BotServices;

using DealBot.Application.Common;
using DealBot.Bot.Resources;
using DealBot.Infrastructure.Persistance.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public partial class BotUpdateHandler(
    ILogger<BotUpdateHandler> logger,
    IServiceScopeFactory serviceScopeFactory)
    : IUpdateHandler
{
    private IStringLocalizer<BotLocalizer> localizer = default!;
    private Domain.Entities.User user = default!;
    private IAppDbContext appDbContext = default!;

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

        using var scope = serviceScopeFactory.CreateScope();
        localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<BotLocalizer>>();
        appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        user = await GetUserAsync(update, cancellationToken);
        SetCulture(user.LanguageCode);

        var handlerTask = update.Type switch
        {
            UpdateType.Message => HandleMessageAsync(botClient, update.Message, cancellationToken),
            UpdateType.CallbackQuery => HandleCallbackQueryAsync(botClient, update.CallbackQuery!, cancellationToken),
            _ => HandleUnknownUpdateAsync(botClient, update, cancellationToken)
        };

        try
        {
            await handlerTask;
            await appDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) { await HandlePollingErrorAsync(botClient, ex, cancellationToken); }
    }

    private async Task<Domain.Entities.User> GetUserAsync(Update update, CancellationToken cancellationToken)
    {
        var updateContent = GetUpdateContent(update);
        var from = updateContent.From;
        long telegramId = from.Id;

        var entity = await appDbContext.Users
            .Include(user => user.Address)
            .Include(user => user.Contact)
            .Include(user => user.Image)
            .Include(user => user.ReferredBy)
            .Include(user => user.Store)
            .Include(user => user.Transactions)
            .Include(user => user.Reviews)
            .Include(user => user.Card)
            .Include(user => user.ReferralsInitiated)
            .FirstOrDefaultAsync(user
                => user.TelegramId.Equals(telegramId), cancellationToken);

        if (entity is null)
            await appDbContext.Users.AddAsync(entity = new Domain.Entities.User
            {
                TelegramId = from.Id,
                LastName = from.LastName,
                Username = from.Username,
                FirstName = from.FirstName,
                LanguageCode = from.LanguageCode,
                Contact = new Domain.Entities.Contact(),
                Card = new Domain.Entities.Card(),
            }, cancellationToken);

        return entity;
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
            CallbackData.CultureUz => new CultureInfo("uz-UZ"),
            CallbackData.CultureEn => new CultureInfo("en-US"),
            CallbackData.CultureRu => new CultureInfo("ru-RU"),
            _ => CultureInfo.CurrentCulture
        };

        CultureInfo.CurrentCulture = new CultureInfo("uz-UZ"); // culture
        CultureInfo.CurrentUICulture = new CultureInfo("uz-UZ"); // culture
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
