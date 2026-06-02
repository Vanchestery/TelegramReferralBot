using ReferralBot.Core.Models;

namespace ReferralBot.Core.Interfaces;

public interface ITelegramBotUserStatesService
{
    Task<TelegramBotUserState?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default);
    Task AddOrUpdateAsync(TelegramBotUserState state, CancellationToken ct = default);
}
