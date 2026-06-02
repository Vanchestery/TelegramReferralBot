namespace ReferralBot.Db.Entities;

/// <summary>
/// Telegram-профиль пользователя.
/// PK = Telegram User ID (задаётся явно, ValueGeneratedNever).
/// Хранит только публичные данные из Telegram — имя, username, флаг партнёра.
/// </summary>
public class TelegramBotUserEntity
{
    /// <summary>Telegram User ID. Первичный ключ, не генерируется БД.</summary>
    public long Id { get; set; }

    /// <summary>@username. Может быть null — не все пользователи его указывают.</summary>
    public string? Username { get; set; }

    /// <summary>Имя из профиля Telegram.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Фамилия из профиля Telegram.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Является ли пользователь партнёром реферальной программы.</summary>
    public bool IsPartner { get; set; } = false;
}
