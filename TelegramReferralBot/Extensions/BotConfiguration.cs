namespace ReferralBot.Extensions;

/// <summary>
/// Конфигурация Telegram-бота.
/// Значения берутся из переменных окружения через ContainerConfigurator.
/// </summary>
public class BotConfiguration
{
    public string Token { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
}
