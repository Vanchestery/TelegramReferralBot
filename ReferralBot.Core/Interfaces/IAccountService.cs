using ReferralBot.Core.Models;

namespace ReferralBot.Core.Interfaces;

public interface IAccountService
{
    Task<Account?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default);
    Task<Account> GetOrCreateAsync(long telegramUserId, CancellationToken ct = default);
    Task AddOrUpdateAsync(Account account, CancellationToken ct = default);
    Task<bool> AddReferrerIdByTelegramIdAsync(long telegramUserId, Guid referrerAccountId, CancellationToken ct = default);
    Task<bool> IsUserReferredAsync(long telegramUserId, CancellationToken ct = default);
}
