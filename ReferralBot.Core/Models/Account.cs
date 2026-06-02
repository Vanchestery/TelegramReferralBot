namespace ReferralBot.Core.Models;

/// <summary>
/// Доменная модель финансового аккаунта пользователя.
/// </summary>
public class Account
{
    public Guid Id { get; set; }
    public long TelegramUserId { get; set; }
    public int BonusBalance { get; set; }
    public Guid? ReferrerId { get; set; }
    public bool IsPartner { get; set; }
    public UserStatus Status { get; set; }
    public int InvitedPurchasesCount { get; set; }
    public int TotalBonusEarned { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
