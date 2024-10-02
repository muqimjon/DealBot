namespace DealBot.Bot.BotServices;

using DealBot.Bot.Resources;
using DealBot.Domain.Enums;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public partial class BotUpdateHandler
{
    private const int minAge = 14;
    private const int count = 16;
    private async Task SendRequestDateOfBirthAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup keyboard = new(new InlineKeyboardButton[][]
        {
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Year], CallbackData.Year),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Month], CallbackData.Month),
                    InlineKeyboardButton.WithCallbackData(localizer[Text.Day], CallbackData.Day)],
            [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)],
        });

        Message sentMessage = await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: localizer[Text.AskDateOfBirth, user.DateOfBirth.ToString("yyyy-MM-dd")],
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectDateOfBirth;
    }

    private async Task HandleDateOfBirthAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(callbackQuery));

        await (callbackQuery.Data switch
        {
            CallbackData.Year => HandleYearAsync(botClient, callbackQuery, cancellationToken),
            CallbackData.Month => SendMonthsAsync(botClient, callbackQuery.Message, cancellationToken),
            CallbackData.Day => SendDaysAsync(botClient, callbackQuery.Message, cancellationToken),
            _ => HandleUnknownCallbackQueryAsync(botClient, callbackQuery, cancellationToken),
        });
    }

    private async Task HandleYearAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(callbackQuery.Message));
        ArgumentNullException.ThrowIfNull(callbackQuery.Data);

        switch (callbackQuery.Data)
        {
            case CallbackData.Next:
                await SendNextYearsAsync(botClient, callbackQuery, cancellationToken);
                break;

            case CallbackData.Previous:
                await SendPreviousYearsAsync(botClient, callbackQuery, cancellationToken);
                break;

            case CallbackData.Year:
                await SendNextYearsAsync(botClient, callbackQuery, cancellationToken);
                break;
            default:
                var year = int.Parse(callbackQuery.Data);
                var inpupDate = DateTime.DaysInMonth(year, user.DateOfBirth.Month);
                var existDate = DateTime.DaysInMonth(user.DateOfBirth.Year, user.DateOfBirth.Month);
                var day = Math.Min(inpupDate, existDate);
                user.DateOfBirth = new DateTimeOffset(year, user.DateOfBirth.Month, day, 0, 0, 0, TimeSpan.Zero);

                await SendRequestDateOfBirthAsync(botClient, callbackQuery.Message, cancellationToken);
                break;
        }
    }

    private async Task SendPreviousYearsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (user.State.Equals(States.WaitingForSelectDateOfBirthYear5))
            return;

        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        var date = DateTime.UtcNow.Year - minAge;
        var page = (int)user.State - (int)States.WaitingForSelectDateOfBirthYear1 + 2;

        List<InlineKeyboardButton[]> buttons = GenerateNumberButtons(date - count * page);
        buttons.AddRange(GenerateNavigationButtons());

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: localizer[Text.AskDateOfBirth, user.DateOfBirth.ToString("yyyy-MM-dd")],
            replyMarkup: new(buttons),
            cancellationToken: cancellationToken
        );

        user.MessageId = sentMessage.MessageId;
        user.State += 1;
    }

    private async Task SendNextYearsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (user.State.Equals(States.WaitingForSelectDateOfBirthYear1))
            return;
        else if (user.State.Equals(States.WaitingForSelectDateOfBirth))
            user.State = States.WaitingForSelectDateOfBirthYear2;

        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));

        var date = DateTime.UtcNow.Year - minAge;
        var page = (int)user.State - (int)States.WaitingForSelectDateOfBirthYear1;

        List<InlineKeyboardButton[]> buttons = GenerateNumberButtons(date - count * page);
        buttons.AddRange(GenerateNavigationButtons());

        var sentMessage = await botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: localizer[Text.AskDateOfBirth, user.DateOfBirth.ToString("yyyy-MM-dd")],
            replyMarkup: new(buttons),
            cancellationToken: cancellationToken
        );

        user.MessageId = sentMessage.MessageId;
        user.State -= 1;
    }

    private async Task SendMonthsAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var months = Enumerable.Range(1, 12)
            .Select(month => new DateTime(1, month, 1).ToString("MMMM", new CultureInfo(user.LanguageCode)))
            .Select((monthName, index) => InlineKeyboardButton.WithCallbackData(monthName, (1 + index).ToString()))
            .Chunk(3)
            .Select(chunk => chunk.ToArray())
            .ToList();

        months.Add([InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]);

        Message sentMessage = await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: localizer[Text.AskDateOfBirth, user.DateOfBirth.ToString("yyyy-MM-dd")],
                replyMarkup: new(months),
                cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectDateOfBirthMonth;
    }

    private async Task HandleMonthAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));
        ArgumentNullException.ThrowIfNull(callbackQuery.Data);

        var month = int.Parse(callbackQuery.Data);
        var inpupDate = DateTime.DaysInMonth(user.DateOfBirth.Year, month);
        var existDate = DateTime.DaysInMonth(user.DateOfBirth.Year, user.DateOfBirth.Month);
        var day = Math.Min(inpupDate, existDate);
        user.DateOfBirth = new DateTimeOffset(user.DateOfBirth.Year, month, day, 0, 0, 0, TimeSpan.Zero);

        await SendRequestDateOfBirthAsync(botClient, callbackQuery.Message, cancellationToken);
    }

    private async Task SendDaysAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        int daysInMonth = DateTime.DaysInMonth(user.DateOfBirth.Year, user.DateOfBirth.Month);
        var buttons = GenerateNumberButtons(1, daysInMonth, 6);
        buttons.Add([InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]);

        Message sentMessage = await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: localizer[Text.AskDateOfBirth, user.DateOfBirth.ToString("yyyy-MM-dd")],
                replyMarkup: new(buttons),
                cancellationToken: cancellationToken);

        user.MessageId = sentMessage.MessageId;
        user.State = States.WaitingForSelectDateOfBirthDay;
    }

    private async Task HandleDayAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(callbackQuery.Message, nameof(Message));
        ArgumentNullException.ThrowIfNull(callbackQuery.Data);

        int day = Math.Min(int.Parse(callbackQuery.Data), DateTime.DaysInMonth(user.DateOfBirth.Year, user.DateOfBirth.Month));
        user.DateOfBirth = new DateTimeOffset(user.DateOfBirth.Year, user.DateOfBirth.Month, day, 0, 0, 0, TimeSpan.Zero);

        await SendRequestDateOfBirthAsync(botClient, callbackQuery.Message, cancellationToken);
    }

    private List<InlineKeyboardButton[]> GenerateNumberButtons(int startNumber, int count = 16, int columns = 4)
        => Enumerable.Range(startNumber, count)
            .Select(number => InlineKeyboardButton.WithCallbackData(number.ToString(), number.ToString()))
            .Chunk(columns)
            .Select(chunk => chunk.ToArray())
            .ToList();

    private List<InlineKeyboardButton[]> GenerateNavigationButtons()
        => [[InlineKeyboardButton.WithCallbackData(localizer[Text.Previous], CallbackData.Previous),
                InlineKeyboardButton.WithCallbackData(localizer[Text.Next], CallbackData.Next)],
        [InlineKeyboardButton.WithCallbackData(localizer[Text.Back], CallbackData.Back)]];
}
