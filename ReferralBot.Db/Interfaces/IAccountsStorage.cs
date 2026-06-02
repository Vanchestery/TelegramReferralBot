using ReferralBot.Db.Entities;

namespace ReferralBot.Db.Interfaces;

public interface IAccountsStorage
{
    Task<AccountEntity?> GetByIdAsync(Guid accountId, CancellationToken ct = default);
    Task<AccountEntity?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default);
    Task UpsertAsync(AccountEntity entity, CancellationToken ct = default);
    Task DeleteByIdAsync(Guid accountId, CancellationToken ct = default);
    Task<bool> IsUserReferredAsync(long telegramUserId, CancellationToken ct = default);
}
