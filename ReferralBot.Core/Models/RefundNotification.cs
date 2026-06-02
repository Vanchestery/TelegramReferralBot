namespace ReferralBot.Core.Models;

/// <summary>
/// Уведомление о возврате от внешней платёжной системы.
/// Принимается через POST /api/bonus/refund.
/// </summary>
public class RefundNotification
{
    public int TransactionId { get; set; }
    public int CourseId { get; set; }
    public long UserId { get; set; }
    public int Amount { get; set; }
    public DateTime RefundTime { get; set; }
}
