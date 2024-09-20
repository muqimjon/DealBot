using DealBot.Application;
using DealBot.Bot.BotServices;
using DealBot.Bot.BotServices.Commons;
using DealBot.Infrastructure;
using Telegram.Bot;
using Telegram.Bot.Polling;

var builder = WebApplication.CreateBuilder(args);

//  ------------------------------  custom
builder.Services.AddApplication();
builder.Services.AddInfrastructureServices(builder.Configuration);

// -------------------------------  manual
builder.Services.AddSingleton(new TelegramBotClient(token:
    builder.Configuration.GetConnectionString("BotToken")!));
builder.Services.AddSingleton<IUpdateHandler, BotUpdateHandler>();
builder.Services.AddHostedService<BotBackgroundService>();
builder.Services.AddLocalization();

var app = builder.Build();

// -------------------------------  manual
var supportedCultures = new[] { "uz", "ru", "en" };
var localizationOptions = new RequestLocalizationOptions()
  .SetDefaultCulture(defaultCulture: supportedCultures[0])
  .AddSupportedCultures(cultures: supportedCultures)
  .AddSupportedUICultures(uiCultures: supportedCultures);
app.UseRequestLocalization(options: localizationOptions);

app.Run();
