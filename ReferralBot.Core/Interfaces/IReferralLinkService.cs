using ReferralBot.Core.Models;

namespace ReferralBot.Core.Interfaces;

public interface IReferralLinkService
{
    Task<ReferralLink> GetOrCreateAsync(long telegramUserId, CancellationToken ct = default);
    Task<ReferralLink?> CheckSecretKeyAsync(string key, CancellationToken ct = default);
}
