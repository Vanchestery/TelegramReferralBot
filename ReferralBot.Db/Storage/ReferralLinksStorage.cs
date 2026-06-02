using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ReferralBot.Db.Context;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Db.Storage;

public class ReferralLinksStorage(
    DatabaseContext db,
    ILogger<ReferralLinksStorage> logger) : IReferralLinksStorage
{
    public async Task<ReferralLinkEntity?> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting ReferralLink by AccountId: {Id}", accountId);

        return await db.ReferralLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.AccountId == accountId, ct);
    }

    public async Task<ReferralLinkEntity?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        logger.LogDebug("Getting ReferralLink by Key: {Key}", key);

        return await db.ReferralLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Key == key, ct);
    }

    public async Task AddAsync(ReferralLinkEntity entity, CancellationToken ct = default)
    {
        logger.LogDebug("Adding ReferralLink for AccountId: {AccountId}", entity.AccountId);

        await db.ReferralLinks.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Added ReferralLink: {Id} for AccountId: {AccountId}", entity.Id, entity.AccountId);
    }
}
