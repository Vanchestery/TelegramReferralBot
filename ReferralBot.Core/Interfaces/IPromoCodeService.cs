namespace ReferralBot.Core.Interfaces;

/// <summary>
/// Промокоды партнёров для оплаты курсов со скидкой.
/// </summary>
public interface IPromoCodeService
{
    /// <summary>
    /// Hex персонального промокода партнёра для ссылки оплаты курса со скидкой.
    /// null — если у аккаунта нет промокода на этот курс (тогда оплата без скидки).
    /// </summary>
    Task<string?> GetHexForPaymentAsync(int courseId, Guid accountId, CancellationToken ct = default);
}
