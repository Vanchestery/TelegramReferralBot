using ReferralBot.Db.Entities;

namespace ReferralBot.Db.Interfaces;

public interface IReferralLinksStorage
{
    Task<ReferralLinkEntity?> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<ReferralLinkEntity?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task AddAsync(ReferralLinkEntity entity, CancellationToken ct = default);
}
