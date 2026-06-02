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
    [HttpPost("/webhook/update")]
    public async Task<IActionResult> Update([FromBody] Update update, CancellationToken ct)
    {
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
