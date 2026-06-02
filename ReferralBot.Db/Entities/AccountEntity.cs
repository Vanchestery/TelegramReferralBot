namespace ReferralBot.Db.Entities;

/// <summary>
/// Финансовый аккаунт пользователя в реферальной системе.
/// Намеренно отделён от TelegramBotUserEntity — профиль и финансы это разные контексты.
/// </summary>
public class AccountEntity
{
    /// <summary>Уникальный идентификатор аккаунта. Генерируется БД через gen_random_uuid().</summary>
    public Guid Id { get; set; }

    /// <summary>Telegram User ID владельца аккаунта.</summary>
    public long TelegramUserId { get; set; }

    /// <summary>Текущий бонусный баланс в бонусных рублях.</summary>
    public int BonusBalance { get; set; } = 0;

    /// <summary>
    /// AccountId пользователя, который пригласил данного пользователя.
    /// null — пользователь пришёл без реферальной ссылки.
    /// </summary>
    public Guid? ReferrerId { get; set; }

    /// <summary>Является ли пользователь партнёром.</summary>
    public bool IsPartner { get; set; } = false;

    /// <summary>Статус аккаунта. Хранится как строка для читаемости в БД.</summary>
    public UserDbStatus Status { get; set; } = UserDbStatus.Active;

    /// <summary>Сколько рефералов данного партнёра совершили покупку курса.</summary>
    public int InvitedPurchasesCount { get; set; } = 0;

    /// <summary>Суммарный доход партнёра за всё время (включая уже потраченные бонусы).</summary>
    public int TotalBonusEarned { get; set; } = 0;

    /// <summary>Дата создания аккаунта. Устанавливается БД через NOW().</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Дата последнего обновления. Обновляется автоматически при любом изменении.</summary>
    public DateTime? UpdatedDate { get; set; }
}
