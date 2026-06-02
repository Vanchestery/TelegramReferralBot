using ReferralBot.Db.Entities;

namespace ReferralBot.Db.Interfaces;

public interface ITelegramUserStatesStorage
{
    Task<TelegramBotUserStateEntity?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default);
    Task UpsertAsync(TelegramBotUserStateEntity entity, CancellationToken ct = default);
}
