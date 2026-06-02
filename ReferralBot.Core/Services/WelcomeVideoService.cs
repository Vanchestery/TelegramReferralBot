using Microsoft.Extensions.Logging;

using ReferralBot.Core.Interfaces;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Core.Services;

public class WelcomeVideoService(
    IWelcomeVideoStorage storage,
    ILogger<WelcomeVideoService> logger) : IWelcomeVideoService
{
    public async Task<string?> GetActiveFileIdAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Getting active welcome video FileId");

        var entity = await storage.GetActiveAsync(ct);

        if (entity is null)
        {
            logger.LogWarning("No active welcome video found");
            return null;
        }

        return entity.FileId;
    }
}
