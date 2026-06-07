using Microsoft.Extensions.Logging;

using ReferralBot.Core.Interfaces;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Core.Services;

public class PromoCodeService(
    IPromoCodesStorage promoCodesStorage,
    ILogger<PromoCodeService> logger) : IPromoCodeService
{
    public async Task<string?> GetHexForPaymentAsync(int courseId, Guid accountId, CancellationToken ct = default)
    {
        var promo = await promoCodesStorage.GetByAccountAndCourseAsync(accountId, courseId, ct);

        if (promo is null || string.IsNullOrEmpty(promo.Hex))
        {
            logger.LogDebug("Нет промокода для курса {CourseId}, аккаунт {AccountId}", courseId, accountId);
            return null;
        }

        return promo.Hex;
    }
}
