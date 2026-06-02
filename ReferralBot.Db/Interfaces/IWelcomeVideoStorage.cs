using ReferralBot.Db.Entities;

namespace ReferralBot.Db.Interfaces;

public interface IWelcomeVideoStorage
{
    Task<WelcomeVideoEntity?> GetActiveAsync(CancellationToken ct = default);
}
