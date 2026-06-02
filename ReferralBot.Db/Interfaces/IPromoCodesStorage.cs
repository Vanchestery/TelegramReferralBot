using ReferralBot.Db.Entities;

namespace ReferralBot.Db.Interfaces;

public interface IPromoCodesStorage
{
    Task<PromoCodeEntity?> GetByAccountAndCourseAsync(Guid accountId, int courseId, CancellationToken ct = default);
    Task<IEnumerable<PromoCodeEntity>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task AddAsync(PromoCodeEntity entity, CancellationToken ct = default);
    Task UpdateAsync(PromoCodeEntity entity, CancellationToken ct = default);
}
