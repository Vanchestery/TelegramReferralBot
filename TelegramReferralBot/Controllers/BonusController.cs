using Microsoft.AspNetCore.Mvc;

using ReferralBot.Attributes;
using ReferralBot.Core.Interfaces;
using ReferralBot.Core.Models;

namespace ReferralBot.Controllers;

[ApiController]
[Route("api/bonus")]
[PartnersKey]
public class BonusController(
    IBonusService bonusService,
    ILogger<BonusController> logger) : ControllerBase
{
    /// <summary>Получить бонусный баланс по Telegram ID.</summary>
    [HttpGet("balance/{telegramUserId:long}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance(long telegramUserId, CancellationToken ct)
    {
        logger.LogDebug("Getting balance for TelegramUserId: {Id}", telegramUserId);

        try
        {
            var balance = await bonusService.GetBonusBalanceAsync(telegramUserId, ct);
            return Ok(balance);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "User not found" });
        }
    }

    /// <summary>
    /// Уведомление о покупке от платёжной системы.
    /// Начисляет бонусы рефереру покупателя.
    /// </summary>
    [HttpPost("payment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment(
        [FromBody] PaymentNotification notification, CancellationToken ct)
    {
        logger.LogDebug("Processing payment, TransactionId: {Id}", notification.TransactionId);

        var success = await bonusService.ProcessPaymentNotificationAsync(notification, ct);

        return success
            ? Ok(new { message = "Payment processed successfully" })
            : BadRequest(new { message = "Failed to process payment" });
    }

    /// <summary>
    /// Уведомление о возврате от платёжной системы.
    /// Отменяет ранее начисленные бонусы.
    /// </summary>
    [HttpPost("refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessRefund(
        [FromBody] RefundNotification notification, CancellationToken ct)
    {
        logger.LogDebug("Processing refund, TransactionId: {Id}", notification.TransactionId);

        var success = await bonusService.ProcessRefundNotificationAsync(notification, ct);

        return success
            ? Ok(new { message = "Refund processed successfully" })
            : BadRequest(new { message = "Failed to process refund" });
    }

    /// <summary>Ручная операция администратора (add / deduct).</summary>
    [HttpPost("manual")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ManualOperation(
        [FromBody] ManualOperation operation, CancellationToken ct)
    {
        if (operation.Type != "add" && operation.Type != "deduct")
            return BadRequest(new { message = "Type must be 'add' or 'deduct'" });

        var success = await bonusService.ProcessManualOperationAsync(operation, ct);

        return success
            ? Ok(new { message = "Manual operation applied" })
            : BadRequest(new { message = "Failed to apply manual operation" });
    }

    /// <summary>Начислить бонусы пользователю.</summary>
    [HttpPost("credit/{telegramUserId:long}/{amount:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Credit(long telegramUserId, int amount, CancellationToken ct)
    {
        if (amount <= 0) return BadRequest(new { message = "Amount must be positive" });

        var success = await bonusService.CreditBonusAsync(telegramUserId, amount, ct);

        return success
            ? Ok(new { message = $"Credited {amount} bonus points" })
            : BadRequest(new { message = "User not found" });
    }

    /// <summary>Списать бонусы у пользователя.</summary>
    [HttpPost("debit/{telegramUserId:long}/{amount:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Debit(long telegramUserId, int amount, CancellationToken ct)
    {
        if (amount <= 0) return BadRequest(new { message = "Amount must be positive" });

        var success = await bonusService.DebitBonusAsync(telegramUserId, amount, ct);

        return success
            ? Ok(new { message = $"Debited {amount} bonus points" })
            : BadRequest(new { message = "Insufficient balance or user not found" });
    }
}
