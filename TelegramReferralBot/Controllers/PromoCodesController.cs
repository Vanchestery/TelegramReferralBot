using Microsoft.AspNetCore.Mvc;

using ReferralBot.Attributes;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Controllers;

[ApiController]
[Route("api/promoCodes")]
[PartnersKey]
public class PromoCodesController(
    IPromoCodesStorage promoCodesStorage,
    IAccountsStorage accountsStorage,
    ILogger<PromoCodesController> logger) : ControllerBase
{
    /// <summary>Промокоды партнёра по Telegram ID.</summary>
    [HttpGet("{telegramUserId:long}")]
    public async Task<IActionResult> GetByTelegramId(long telegramUserId, CancellationToken ct)
    {
        logger.LogDebug("Getting promo codes for TelegramUserId: {Id}", telegramUserId);

        var account = await accountsStorage.GetByTelegramUserIdAsync(telegramUserId, ct);
        if (account is null)
            return NotFound(new { message = "Account not found" });

        var codes = await promoCodesStorage.GetByAccountIdAsync(account.Id, ct);
        return Ok(codes);
    }
}
