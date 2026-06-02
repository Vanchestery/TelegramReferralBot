namespace ReferralBot.Core.Models;

/// <summary>
/// Ручная операция с бонусным балансом от администратора.
/// Type: "add" — начислить, "deduct" — списать.
/// </summary>
public class ManualOperation
{
    public long TelegramUserId { get; set; }
    public int Amount { get; set; }

    /// <summary>"add" или "deduct".</summary>
    public string Type { get; set; } = string.Empty;
}
