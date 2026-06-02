using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ReferralBot;

/// <summary>
/// Фоновая служба запускающаяся при старте приложения.
/// Регистрирует webhook URL в Telegram и задаёт список команд бота.
///
/// Используем IHostedService а не BackgroundService — нам нужна только
/// однократная инициализация при старте, без бесконечного цикла.
/// </summary>
public class WebHookConfigurator(
    IServiceScopeFactory scopeFactory,
    ILogger<WebHookConfigurator> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var commandsProvider = scope.ServiceProvider.GetRequiredService<Services.CommandsProvider>();

        var webhookUrl = config["REF_BOT_WEBHOOK_URL"]
            ?? throw new InvalidOperationException("REF_BOT_WEBHOOK_URL is not set");

        var fullWebhookUrl = $"{webhookUrl.TrimEnd('/')}/webhook/update";

        logger.LogInformation("Setting webhook: {Url}", fullWebhookUrl);

        await client.SetWebhook(
            url: fullWebhookUrl,
            allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery],
            cancellationToken: ct);

        await commandsProvider.SetCommandsAsync(client, ct);

        logger.LogInformation("Webhook configured successfully");
    }

    public async Task StopAsync(CancellationToken ct)
    {
        // При остановке удаляем webhook — бот не будет получать обновления
        using var scope = scopeFactory.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        await client.DeleteWebhook(cancellationToken: ct);
        logger.LogInformation("Webhook removed");
    }
}
