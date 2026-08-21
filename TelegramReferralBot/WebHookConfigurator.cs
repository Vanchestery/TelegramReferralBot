using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

using ReferralBot.Extensions;

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

        var webhookUrl = WebhookUrlResolver.Resolve(config)
            ?? throw new InvalidOperationException("Webhook URL is not set (VS_TUNNEL_URL or REF_BOT_WEBHOOK_URL)");

        if (!string.IsNullOrEmpty(config["VS_TUNNEL_URL"]))
            logger.LogInformation("Using Dev Tunnel URL for webhook");

        var fullWebhookUrl = $"{webhookUrl.TrimEnd('/')}/webhook/update";

        logger.LogInformation("Setting webhook: {Url}", fullWebhookUrl);

        // Dev Tunnel + Kestrel иногда не готовы в момент старта — Telegram при setWebhook
        // сразу проверяет URL и падает с RequestException. Retry с паузой решает это.
        var useDevTunnel = !string.IsNullOrEmpty(config["VS_TUNNEL_URL"]);
        const int maxAttempts = 5;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (attempt == 1 && useDevTunnel)
                await Task.Delay(TimeSpan.FromSeconds(3), ct);

            try
            {
                await client.SetWebhook(
                    url: fullWebhookUrl,
                    allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery],
                    cancellationToken: ct);
                break;
            }
            catch (RequestException ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "SetWebhook attempt {Attempt}/{Max} failed ({Reason}). Retrying...",
                    attempt,
                    maxAttempts,
                    ex.InnerException?.Message ?? ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt), ct);
            }
        }

        await commandsProvider.SetCommandsAsync(client, ct);

        logger.LogInformation("Webhook configured successfully");
    }

    public async Task StopAsync(CancellationToken ct)
    {
        if (string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Development: webhook left registered (skip DeleteWebhook on stop)");
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        await client.DeleteWebhook(cancellationToken: ct);
        logger.LogInformation("Webhook removed");
    }
}
