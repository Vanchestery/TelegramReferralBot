using ReferralBot.Core.Models;

namespace ReferralBot.Core.Interfaces;

public interface ITelegramBotUserService
{
    Task<TelegramBotUser?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default);
    Task AddOrUpdateAsync(TelegramBotUser user, CancellationToken ct = default);
    Task UpdatePartnerStatusAsync(long telegramUserId, bool isPartner, CancellationToken ct = default);
}
