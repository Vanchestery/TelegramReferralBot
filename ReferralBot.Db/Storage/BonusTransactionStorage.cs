using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ReferralBot.Db.Context;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Db.Storage;

public class BonusTransactionStorage(
    DatabaseContext db,
    ILogger<BonusTransactionStorage> logger) : IBonusTransactionStorage
{
    public async Task<BonusTransactionEntity?> GetByPaymentTransactionIdAsync(int paymentTransactionId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting BonusTransaction by PaymentTransactionId: {Id}", paymentTransactionId);

        return await db.BonusTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.PaymentTransactionId == paymentTransactionId, ct);
    }

    public async Task<IEnumerable<BonusTransactionEntity>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting BonusTransactions for AccountId: {Id}", accountId);

        return await db.BonusTransactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(BonusTransactionEntity entity, CancellationToken ct = default)
    {
        logger.LogDebug("Adding BonusTransaction for AccountId: {AccountId}, Amount: {Amount}", entity.AccountId, entity.Amount);

        await db.BonusTransactions.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Added BonusTransaction: {Id}, Type: {Type}, Amount: {Amount}", entity.Id, entity.OperationType, entity.Amount);
    }
}
