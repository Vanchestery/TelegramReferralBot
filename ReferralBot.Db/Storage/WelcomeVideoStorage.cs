using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ReferralBot.Db.Context;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Db.Storage;

public class WelcomeVideoStorage(
    DatabaseContext db,
    ILogger<WelcomeVideoStorage> logger) : IWelcomeVideoStorage
{
    public async Task<WelcomeVideoEntity?> GetActiveAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Getting active WelcomeVideo");

        return await db.WelcomeVideos
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.IsActive, ct);
    }
}
