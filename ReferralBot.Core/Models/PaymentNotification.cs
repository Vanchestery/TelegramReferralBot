namespace ReferralBot.Core.Models;

/// <summary>
/// Уведомление о покупке от внешней платёжной системы (Stepik).
/// Принимается через POST /api/bonus/payment.
/// </summary>
public class PaymentNotification
{
    public int TransactionId { get; set; }
    public int CourseId { get; set; }

    /// <summary>Stepik User ID покупателя.</summary>
    public long UserId { get; set; }

    /// <summary>Сумма покупки в рублях.</summary>
    public int Amount { get; set; }

    public DateTime PaymentTime { get; set; }
}
