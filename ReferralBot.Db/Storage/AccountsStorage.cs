using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ReferralBot.Db.Context;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Db.Storage;

public class AccountsStorage(
    DatabaseContext db,
    ILogger<AccountsStorage> logger) : IAccountsStorage
{
    public async Task<AccountEntity?> GetByIdAsync(Guid accountId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting Account by Id: {Id}", accountId);

        return await db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId, ct);
    }

    public async Task<AccountEntity?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting Account by TelegramUserId: {Id}", telegramUserId);

        return await db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TelegramUserId == telegramUserId, ct);
    }

    public async Task UpsertAsync(AccountEntity entity, CancellationToken ct = default)
    {
        logger.LogDebug("Upserting Account: {Id}", entity.Id);

        var existing = await db.Accounts
            .FirstOrDefaultAsync(a => a.Id == entity.Id, ct);

        if (existing is null)
        {
            await db.Accounts.AddAsync(entity, ct);
        }
        else
        {
            existing.BonusBalance = entity.BonusBalance;
            existing.ReferrerId = entity.ReferrerId;
            existing.IsPartner = entity.IsPartner;
            existing.Status = entity.Status;
            existing.InvitedPurchasesCount = entity.InvitedPurchasesCount;
            existing.TotalBonusEarned = entity.TotalBonusEarned;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Upserted Account: {Id}", entity.Id);
    }

    public async Task DeleteByIdAsync(Guid accountId, CancellationToken ct = default)
    {
        logger.LogDebug("Deleting Account: {Id}", accountId);

        var entity = await db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, ct);

        if (entity is null)
        {
            logger.LogWarning("Account not found for deletion: {Id}", accountId);
            return;
        }

        db.Accounts.Remove(entity);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted Account: {Id}", accountId);
    }

    public async Task<bool> IsUserReferredAsync(long telegramUserId, CancellationToken ct = default)
    {
        return await db.Accounts
            .AsNoTracking()
            .AnyAsync(a => a.TelegramUserId == telegramUserId && a.ReferrerId != null, ct);
    }
}
