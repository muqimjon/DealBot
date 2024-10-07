﻿namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

public partial class BotUpdateHandler
{
    private async Task HandleTextMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message is null || message.Text is null)
            return;

        else if (message.Text.Equals(localizer[Text.Back]))
            await NavigateToPreviousPageAsync(botClient, message, cancellationToken);

        var userState = message.Text.Equals("/start") && !user.State.Equals(States.None) ? States.Restart : user.State;

        var handler = user.Role switch
        {
            Roles.Customer => userState switch
            {
                States.None => SendFirstMenuLanguagesAsync(botClient, message, cancellationToken),
                States.Restart => SendCustomerMenuAsync(botClient, message, cancellationToken),
                States.WaitingForSendComment => HandleCommentMessageAsync(botClient, message, cancellationToken),
                States.WaitingForSendEmail => HandleEmailAsync(botClient, message, cancellationToken),
                States.WaitingForSendFirstName => HandleFirstNameAsync(botClient, message, cancellationToken),
                States.WaitingForSendLastName => HandleLastNameAsync(botClient, message, cancellationToken),
                _ => HandleUnknownMessageAsync(botClient, message, cancellationToken)
            },
            Roles.Seller => userState switch
            {
                States.None => SendFirstMenuLanguagesAsync(botClient, message, cancellationToken),
                States.Restart => SendSellerMenuAsync(botClient, message, cancellationToken),
                _ => HandleUnknownMessageAsync(botClient, message, cancellationToken)
            },
            _ => default!
        };

        try { await handler; }
        catch (Exception ex) { logger.LogError(ex, "Error handling message from {user.FirstName}", user.FirstName); }
    }
}
