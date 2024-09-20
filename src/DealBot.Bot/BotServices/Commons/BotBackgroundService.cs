namespace DealBot.Bot.BotServices.Commons;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;

public class BotBackgroundService(TelegramBotClient client,
    ILogger<BotBackgroundService> logger,
    IUpdateHandler handler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var botInfo = await client.GetMeAsync(cancellationToken);
            logger.LogInformation("Bot {BotName} started successfully", botInfo.Username);

            client.StartReceiving(
                handler.HandleUpdateAsync,
                handler.HandlePollingErrorAsync,
                receiverOptions: new ReceiverOptions(),
                cancellationToken: cancellationToken);

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Bot service cancellation requested.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while starting the bot.");
        }
    }
}
