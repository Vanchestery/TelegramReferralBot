using Microsoft.Extensions.Logging;

using ReferralBot.Core.Interfaces;
using ReferralBot.Core.Models;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Core.Services;

public class BonusService(
    IAccountsStorage accountsStorage,
    IBonusTransactionStorage transactionStorage,
    IPartnerService partnerService,
    ILogger<BonusService> logger) : IBonusService
{
    public async Task<int> GetBonusBalanceAsync(long telegramUserId, CancellationToken ct = default)
    {
        var account = await accountsStorage.GetByTelegramUserIdAsync(telegramUserId, ct);

        if (account is null)
        {
            logger.LogWarning("Account not found for TelegramUserId: {Id}", telegramUserId);
            throw new InvalidOperationException($"Account not found for TelegramUserId: {telegramUserId}");
        }

        return account.BonusBalance;
    }

    public async Task<bool> ProcessPaymentNotificationAsync(PaymentNotification notification, CancellationToken ct = default)
    {
        logger.LogDebug("Processing payment notification, TransactionId: {Id}", notification.TransactionId);

        // Idempotency: не обрабатываем одну транзакцию дважды
        var existing = await transactionStorage.GetByPaymentTransactionIdAsync(notification.TransactionId, ct);
        if (existing is not null)
        {
            logger.LogInformation("Payment TransactionId {Id} already processed, skipping", notification.TransactionId);
            return true;
        }

        var buyerAccount = await accountsStorage.GetByTelegramUserIdAsync(notification.UserId, ct);
        if (buyerAccount is null)
        {
            logger.LogWarning("Buyer account not found for UserId: {Id}", notification.UserId);
            return false;
        }

        // Начисляем бонусы только если у покупателя есть реферер
        if (buyerAccount.ReferrerId is null)
        {
            logger.LogDebug("No referrer for UserId: {Id}, skipping bonus accrual", notification.UserId);
            return true;
        }

        var referrerAccount = await accountsStorage.GetByIdAsync(buyerAccount.ReferrerId.Value, ct);
        if (referrerAccount is null)
        {
            logger.LogWarning("Referrer account not found: {ReferrerId}", buyerAccount.ReferrerId);
            return false;
        }

        var level = CalculateLevel(referrerAccount.InvitedPurchasesCount);
        var rate = GetBonusRate(level);
        var bonusAmount = notification.Amount * rate / 100;

        var transaction = new BonusTransactionEntity
        {
            Id = Guid.NewGuid(),
            AccountId = referrerAccount.Id,
            Amount = bonusAmount,
            PaymentTransactionId = notification.TransactionId,
            PaymentTime = notification.PaymentTime,
            OperationType = Db.Entities.BonusOperationType.Purchase,
            CreatedDate = DateTime.UtcNow,
            BalanceBefore = referrerAccount.BonusBalance,
            BalanceAfter = referrerAccount.BonusBalance + bonusAmount,
            PurchasedCourseId = notification.CourseId
        };

        referrerAccount.BonusBalance += bonusAmount;
        referrerAccount.TotalBonusEarned += bonusAmount;
        referrerAccount.InvitedPurchasesCount += 1;

        await transactionStorage.AddAsync(transaction, ct);
        await accountsStorage.UpsertAsync(referrerAccount, ct);
        partnerService.InvalidateProfileCache(referrerAccount.TelegramUserId);

        logger.LogInformation("Accrued {Amount} bonus points to AccountId: {AccountId}", bonusAmount, referrerAccount.Id);
        return true;
    }

    public async Task<bool> ProcessRefundNotificationAsync(RefundNotification notification, CancellationToken ct = default)
    {
        logger.LogDebug("Processing refund notification, TransactionId: {Id}", notification.TransactionId);

        var originalTransaction = await transactionStorage.GetByPaymentTransactionIdAsync(notification.TransactionId, ct);
        if (originalTransaction is null)
        {
            logger.LogWarning("Original transaction not found for refund, TransactionId: {Id}", notification.TransactionId);
            return false;
        }

        var account = await accountsStorage.GetByIdAsync(originalTransaction.AccountId, ct);
        if (account is null)
        {
            logger.LogWarning("Account not found for refund: {AccountId}", originalTransaction.AccountId);
            return false;
        }

        var refundAmount = originalTransaction.Amount;

        var refundTransaction = new BonusTransactionEntity
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Amount = -refundAmount,
            PaymentTransactionId = notification.TransactionId,
            PaymentTime = notification.RefundTime,
            OperationType = Db.Entities.BonusOperationType.Refund,
            CreatedDate = DateTime.UtcNow,
            BalanceBefore = account.BonusBalance,
            BalanceAfter = account.BonusBalance - refundAmount,
            PurchasedCourseId = notification.CourseId
        };

        account.BonusBalance = Math.Max(0, account.BonusBalance - refundAmount);
        account.InvitedPurchasesCount = Math.Max(0, account.InvitedPurchasesCount - 1);

        await transactionStorage.AddAsync(refundTransaction, ct);
        await accountsStorage.UpsertAsync(account, ct);
        partnerService.InvalidateProfileCache(account.TelegramUserId);

        logger.LogInformation("Processed refund of {Amount} for AccountId: {AccountId}", refundAmount, account.Id);
        return true;
    }

    public async Task<bool> ProcessManualOperationAsync(ManualOperation operation, CancellationToken ct = default)
    {
        logger.LogDebug("Processing manual operation for TelegramUserId: {Id}, Type: {Type}, Amount: {Amount}",
            operation.TelegramUserId, operation.Type, operation.Amount);

        return operation.Type == "add"
            ? await CreditBonusAsync(operation.TelegramUserId, operation.Amount, ct)
            : await DebitBonusAsync(operation.TelegramUserId, operation.Amount, ct);
    }

    public async Task<bool> CreditBonusAsync(long telegramUserId, int amount, CancellationToken ct = default)
    {
        var account = await accountsStorage.GetByTelegramUserIdAsync(telegramUserId, ct);
        if (account is null) return false;

        var transaction = new BonusTransactionEntity
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Amount = amount,
            PaymentTransactionId = 0,
            PaymentTime = DateTime.UtcNow,
            OperationType = Db.Entities.BonusOperationType.Credit,
            CreatedDate = DateTime.UtcNow,
            BalanceBefore = account.BonusBalance,
            BalanceAfter = account.BonusBalance + amount
        };

        account.BonusBalance += amount;
        account.TotalBonusEarned += amount;

        await transactionStorage.AddAsync(transaction, ct);
        await accountsStorage.UpsertAsync(account, ct);
        partnerService.InvalidateProfileCache(telegramUserId);

        logger.LogInformation("Credited {Amount} to TelegramUserId: {Id}", amount, telegramUserId);
        return true;
    }

    public async Task<bool> DebitBonusAsync(long telegramUserId, int amount, CancellationToken ct = default)
    {
        var account = await accountsStorage.GetByTelegramUserIdAsync(telegramUserId, ct);
        if (account is null) return false;

        if (account.BonusBalance < amount)
        {
            logger.LogWarning("Insufficient balance for TelegramUserId: {Id}. Balance: {Balance}, Requested: {Amount}",
                telegramUserId, account.BonusBalance, amount);
            return false;
        }

        var transaction = new BonusTransactionEntity
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Amount = -amount,
            PaymentTransactionId = 0,
            PaymentTime = DateTime.UtcNow,
            OperationType = Db.Entities.BonusOperationType.Debit,
            CreatedDate = DateTime.UtcNow,
            BalanceBefore = account.BonusBalance,
            BalanceAfter = account.BonusBalance - amount
        };

        account.BonusBalance -= amount;

        await transactionStorage.AddAsync(transaction, ct);
        await accountsStorage.UpsertAsync(account, ct);
        partnerService.InvalidateProfileCache(telegramUserId);

        logger.LogInformation("Debited {Amount} from TelegramUserId: {Id}", amount, telegramUserId);
        return true;
    }

    private static UserLevel CalculateLevel(int invitedPurchasesCount) => invitedPurchasesCount switch
    {
        <= 2 => UserLevel.Intern,
        <= 5 => UserLevel.Junior,
        <= 10 => UserLevel.Middle,
        <= 20 => UserLevel.Senior,
        _ => UserLevel.Ambassador
    };

    private static int GetBonusRate(UserLevel level) => level switch
    {
        UserLevel.Intern => 15,
        UserLevel.Junior => 20,
        UserLevel.Middle => 25,
        UserLevel.Senior => 27,
        UserLevel.Ambassador => 30,
        _ => 15
    };
}
