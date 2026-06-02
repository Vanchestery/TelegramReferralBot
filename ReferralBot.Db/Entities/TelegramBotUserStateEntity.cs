namespace ReferralBot.Db.Entities;

/// <summary>
/// Состояние диалога пользователя с ботом.
/// Хранит стек навигации (список имён типов страниц) в jsonb-колонке.
/// PK = TelegramUserId, один пользователь — одна запись состояния.
/// </summary>
public class TelegramBotUserStateEntity
{
    /// <summary>Telegram User ID. Первичный ключ и unique index.</summary>
    public long TelegramUserId { get; set; }

    /// <summary>
    /// Стек страниц в виде списка полных имён типов (Type.FullName).
    /// Хранится как jsonb. Порядок: первый элемент = дно стека, последний = вершина.
    /// Пример: ["ReferralBot.Pages.StartPage", "ReferralBot.Pages.Partner.PartnerHomePage"]
    /// </summary>
    public List<string> PageNames { get; set; } = [];

    /// <summary>Telegram Message ID последнего сообщения бота — нужен для удаления перед отправкой нового.</summary>
    public int CurrentMessageId { get; set; } = 0;

    /// <summary>Приветственное видео уже было отправлено этому пользователю.</summary>
    public bool IsWelcomeMessageSent { get; set; } = false;

    /// <summary>Последнее сообщение бота содержало медиа (фото/видео) — влияет на метод отправки следующего.</summary>
    public bool IsMediaContent { get; set; } = false;
}
