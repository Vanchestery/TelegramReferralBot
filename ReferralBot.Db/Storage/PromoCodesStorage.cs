using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ReferralBot.Db.Context;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Db.Storage;

public class PromoCodesStorage(
    DatabaseContext db,
    ILogger<PromoCodesStorage> logger) : IPromoCodesStorage
{
    public async Task<PromoCodeEntity?> GetByAccountAndCourseAsync(Guid accountId, int courseId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting PromoCode for AccountId: {AccountId}, CourseId: {CourseId}", accountId, courseId);

        return await db.PromoCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.AccountId == accountId && p.CourseId == courseId, ct);
    }

    public async Task<IEnumerable<PromoCodeEntity>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting PromoCodes for AccountId: {AccountId}", accountId);

        return await db.PromoCodes
            .AsNoTracking()
            .Where(p => p.AccountId == accountId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(PromoCodeEntity entity, CancellationToken ct = default)
    {
        logger.LogDebug("Adding PromoCode for AccountId: {AccountId}, CourseId: {CourseId}", entity.AccountId, entity.CourseId);

        await db.PromoCodes.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Added PromoCode: {Id}", entity.Id);
    }

    public async Task UpdateAsync(PromoCodeEntity entity, CancellationToken ct = default)
    {
        logger.LogDebug("Updating PromoCode: {Id}", entity.Id);

        var existing = await db.PromoCodes.FirstOrDefaultAsync(p => p.Id == entity.Id, ct);

        if (existing is null)
        {
            logger.LogWarning("PromoCode not found for update: {Id}", entity.Id);
            return;
        }

        existing.IsActive = entity.IsActive;
        existing.Discount = entity.Discount;
        existing.Description = entity.Description;
        existing.ExpireDate = entity.ExpireDate;
        existing.UpdateDate = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated PromoCode: {Id}", entity.Id);
    }
}
