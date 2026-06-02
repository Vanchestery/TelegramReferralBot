namespace ReferralBot.Core.Models;

public class TelegramBotUser
{
    public long Id { get; set; }
    public string? Username { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsPartner { get; set; }
}
