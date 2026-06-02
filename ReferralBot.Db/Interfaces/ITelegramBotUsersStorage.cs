using ReferralBot.Db.Entities;

namespace ReferralBot.Db.Interfaces;

public interface ITelegramBotUsersStorage
{
    Task<TelegramBotUserEntity?> GetByIdAsync(long telegramUserId, CancellationToken ct = default);
    Task UpsertAsync(TelegramBotUserEntity entity, CancellationToken ct = default);
    Task UpdatePartnerStatusAsync(long telegramUserId, bool isPartner, CancellationToken ct = default);
}
