using ReferralBot.Core.Models;

namespace ReferralBot.Core.Interfaces;

public interface IBonusService
{
    Task<int> GetBonusBalanceAsync(long telegramUserId, CancellationToken ct = default);
    Task<bool> ProcessPaymentNotificationAsync(PaymentNotification notification, CancellationToken ct = default);
    Task<bool> ProcessRefundNotificationAsync(RefundNotification notification, CancellationToken ct = default);
    Task<bool> ProcessManualOperationAsync(ManualOperation operation, CancellationToken ct = default);
    Task<bool> CreditBonusAsync(long telegramUserId, int amount, CancellationToken ct = default);
    Task<bool> DebitBonusAsync(long telegramUserId, int amount, CancellationToken ct = default);
}
