namespace ReferralBot.Core.Models;

public class ReferralLink
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public DateTime CreatedDate { get; set; }
}
