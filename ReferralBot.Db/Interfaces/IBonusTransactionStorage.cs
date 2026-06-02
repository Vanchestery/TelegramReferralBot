using ReferralBot.Db.Entities;

namespace ReferralBot.Db.Interfaces;

public interface IBonusTransactionStorage
{
    Task<BonusTransactionEntity?> GetByPaymentTransactionIdAsync(int paymentTransactionId, CancellationToken ct = default);
    Task<IEnumerable<BonusTransactionEntity>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task AddAsync(BonusTransactionEntity entity, CancellationToken ct = default);
}
