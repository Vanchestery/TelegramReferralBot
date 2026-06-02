namespace ReferralBot.Core.Interfaces;

public interface IWelcomeVideoService
{
    /// <summary>
    /// Возвращает Telegram file_id активного приветственного видео.
    /// null — если видео не загружено или не активно.
    /// </summary>
    Task<string?> GetActiveFileIdAsync(CancellationToken ct = default);
}
