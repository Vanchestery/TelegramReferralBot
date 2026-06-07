using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

using ReferralBot.Services.Bot;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ReferralBot.Controllers;

[ApiController]
public class WebhookController(
    IBotService botService,
    ITelegramBotClient botClient,
    ILogger<WebhookController> logger) : ControllerBase
{
    /// <summary>
    /// Приёмник обновлений от Telegram.
    /// </summary>
    /// <remarks>
    /// Telegram присылает JSON в snake_case (first_name, message_id, ...).
    /// Стандартный сериализатор ASP.NET Core (camelCase) НЕ мапит эти поля на
    /// свойства типов Telegram.Bot — например, first_name не ложится в FirstName,
    /// из-за чего срабатывает валидация модели и запрос рубится с 400 ещё до сюда.
    ///
    /// Поэтому НЕ используем [FromBody]: читаем тело и десериализуем Update вручную
    /// через JsonBotAPI.Options — это серилазатор самой Telegram.Bot со snake_case
    /// naming policy и полиморфными конвертерами. Глобальный MVC-сериализатор при
    /// этом не трогаем, чтобы REST API (/api/bonus/*) сохранил свой camelCase-контракт.
    /// </remarks>
    [HttpPost("/webhook/update")]
    public async Task<IActionResult> Update(CancellationToken ct)
    {
        Update? update;
        try
        {
            update = await JsonSerializer.DeserializeAsync<Update>(
                Request.Body, JsonBotAPI.Options, ct);
        }
        catch (JsonException ex)
        {
            // Битый/неожиданный payload. Логируем, но возвращаем 200 —
            // иначе Telegram уйдёт в бесконечные ретраи этого же апдейта.
            logger.LogWarning(ex, "Failed to deserialize incoming update");
            return Ok();
        }

        if (update is null)
            return Ok();

        try
        {
            await botService.HandleUpdateAsync(update, botClient, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in webhook");
            await botService.HandleErrorAsync(ex, ct);
        }

        // Telegram требует 200 OK даже при ошибках — иначе будет повторять запрос
        return Ok();
    }
}
