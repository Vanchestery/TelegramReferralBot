using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ReferralBot.Db.Context;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Db.Storage;

public class TelegramUserStatesStorage(
    DatabaseContext db,
    ILogger<TelegramUserStatesStorage> logger) : ITelegramUserStatesStorage
{
    public async Task<TelegramBotUserStateEntity?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting UserState for TelegramUserId: {Id}", telegramUserId);

        return await db.TelegramUserStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TelegramUserId == telegramUserId, ct);
    }

    public async Task UpsertAsync(TelegramBotUserStateEntity entity, CancellationToken ct = default)
    {
        logger.LogDebug("Upserting UserState for TelegramUserId: {Id}", entity.TelegramUserId);

        var existing = await db.TelegramUserStates
            .FirstOrDefaultAsync(s => s.TelegramUserId == entity.TelegramUserId, ct);

        if (existing is null)
        {
            await db.TelegramUserStates.AddAsync(entity, ct);
        }
        else
        {
            existing.PageNames = entity.PageNames;
            existing.CurrentMessageId = entity.CurrentMessageId;
            existing.IsWelcomeMessageSent = entity.IsWelcomeMessageSent;
            existing.IsMediaContent = entity.IsMediaContent;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Upserted UserState for TelegramUserId: {Id}", entity.TelegramUserId);
    }
}
