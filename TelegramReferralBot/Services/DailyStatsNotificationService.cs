using ReferralBot.Core.Interfaces;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ReferralBot.Services;

/// <summary>
/// Фоновая служба для ежедневной рассылки статистики партнёрам.
/// Запускается автоматически при старте приложения и работает бесконечно.
///
/// Ловушка: BackgroundService регистрируется как Singleton.
/// Нельзя инжектировать Scoped-сервисы напрямую в конструктор —
/// только через IServiceScopeFactory внутри цикла ExecuteAsync.
/// </summary>
public class DailyStatsNotificationService(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    ILogger<DailyStatsNotificationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DailyStatsNotificationService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();
            logger.LogDebug("Next daily stats run in {Minutes} minutes", delay.TotalMinutes);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            await SendDailyStatsAsync(stoppingToken);
        }

        logger.LogInformation("DailyStatsNotificationService stopped");
    }

    private async Task SendDailyStatsAsync(CancellationToken ct)
    {
        logger.LogInformation("Starting daily stats notification");

        // Создаём scope для Scoped-сервисов — это обязательный паттерн для BackgroundService
        using var scope = scopeFactory.CreateScope();
        var partnerService = scope.ServiceProvider.GetRequiredService<IPartnerService>();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        try
        {
            var partners = await partnerService.GetAllPartnersAsync(ct);
            var count = 0;

            foreach (var partner in partners)
            {
                try
                {
                    var message =
                        $"📊 *Ежедневная статистика*\n\n" +
                        $"💰 Баланс: {partner.BonusBalance}₽\n" +
                        $"👥 Рефералов: {partner.InvitedCount}\n" +
                        $"✅ Купили курс: {partner.InvitedPurchasesCount}\n" +
                        $"💵 Всего заработано: {partner.TotalBonusEarned}₽";

                    await botClient.SendMessage(
                        chatId: partner.TelegramUserId,
                        text: message,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: ct);

                    count++;

                    // Небольшая задержка между отправками — защита от flood limit Telegram
                    await Task.Delay(50, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to send stats to TelegramUserId: {Id}", partner.TelegramUserId);
                }
            }

            logger.LogInformation("Daily stats sent to {Count} partners", count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during daily stats notification");
        }
    }

    /// <summary>
    /// Вычисляет задержку до следующего запуска (каждый день в 09:00 UTC).
    /// </summary>
    private static TimeSpan GetDelayUntilNextRun()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddHours(9);

        if (now >= nextRun)
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }
}
