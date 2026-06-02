using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ReferralBot.Db.Context;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Db.Storage;

public class TelegramBotUsersStorage(
    DatabaseContext db,
    ILogger<TelegramBotUsersStorage> logger) : ITelegramBotUsersStorage
{
    public async Task<TelegramBotUserEntity?> GetByIdAsync(long telegramUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting TelegramBotUser by Id: {Id}", telegramUserId);

        return await db.TelegramBotUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == telegramUserId, ct);
    }

    public async Task UpsertAsync(TelegramBotUserEntity entity, CancellationToken ct = default)
    {
        logger.LogDebug("Upserting TelegramBotUser: {Id}", entity.Id);

        var existing = await db.TelegramBotUsers
            .FirstOrDefaultAsync(u => u.Id == entity.Id, ct);

        if (existing is null)
        {
            await db.TelegramBotUsers.AddAsync(entity, ct);
        }
        else
        {
            existing.Username = entity.Username;
            existing.FirstName = entity.FirstName;
            existing.LastName = entity.LastName;
            existing.IsPartner = entity.IsPartner;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Upserted TelegramBotUser: {Id}", entity.Id);
    }

    public async Task UpdatePartnerStatusAsync(long telegramUserId, bool isPartner, CancellationToken ct = default)
    {
        logger.LogDebug("Updating partner status for TelegramUserId: {Id}, IsPartner: {IsPartner}", telegramUserId, isPartner);

        var entity = await db.TelegramBotUsers
            .FirstOrDefaultAsync(u => u.Id == telegramUserId, ct);

        if (entity is null)
        {
            logger.LogWarning("TelegramBotUser not found: {Id}", telegramUserId);
            return;
        }

        entity.IsPartner = isPartner;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated partner status for TelegramUserId: {Id}", telegramUserId);
    }

    public async Task<IEnumerable<TelegramBotUserEntity>> GetAllPartnersAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Getting all partner users");

        return await db.TelegramBotUsers
            .AsNoTracking()
            .Where(u => u.IsPartner)
            .ToListAsync(ct);
    }
}
