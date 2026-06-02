namespace ReferralBot.Core.Models;

public class BonusTransaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public int Amount { get; set; }
    public int PaymentTransactionId { get; set; }
    public DateTime PaymentTime { get; set; }
    public BonusOperationType OperationType { get; set; }
    public DateTime CreatedDate { get; set; }
    public int BalanceBefore { get; set; }
    public int BalanceAfter { get; set; }
    public int PurchasedCourseId { get; set; }
}
