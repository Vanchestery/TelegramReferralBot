namespace ReferralBot.Core.Models;

/// <summary>
/// Профиль партнёра для отображения в боте.
/// Агрегирует данные из Account — вычисляет уровень и процент бонусов.
/// </summary>
public class PartnerProfile
{
    public Guid AccountId { get; set; }
    public long TelegramUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public int BonusBalance { get; set; }
    public int TotalBonusEarned { get; set; }
    public int InvitedCount { get; set; }
    public int InvitedPurchasesCount { get; set; }
    public UserLevel Level { get; set; }

    /// <summary>Процент бонусов, начисляемых партнёру с каждой покупки реферала.</summary>
    public int BonusRate => Level switch
    {
        UserLevel.Intern => 15,
        UserLevel.Junior => 20,
        UserLevel.Middle => 25,
        UserLevel.Senior => 27,
        UserLevel.Ambassador => 30,
        _ => 15
    };
}
