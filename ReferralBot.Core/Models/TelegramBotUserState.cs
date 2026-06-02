namespace ReferralBot.Core.Models;

public class TelegramBotUserState
{
    public long TelegramUserId { get; set; }
    public List<string> PageNames { get; set; } = [];
    public int CurrentMessageId { get; set; }
    public bool IsWelcomeMessageSent { get; set; }
    public bool IsMediaContent { get; set; }
}
