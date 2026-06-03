using ReferralBot.Core.Models;

namespace ReferralBot.Core.Interfaces;

public interface IPartnerService
{
    Task<PartnerProfile?> GetProfileAsync(long telegramUserId, CancellationToken ct = default);
    Task<string?> GetReferrerNameByTelegramIdAsync(long telegramUserId, CancellationToken ct = default);
    Task<IEnumerable<PartnerProfile>> GetAllPartnersAsync(CancellationToken ct = default);
    void InvalidateProfileCache(long telegramUserId);
}
