namespace ReferralBot.Db.Entities;

/// <summary>
/// Реферальная ссылка партнёра.
/// Один аккаунт = одна ссылка (unique index на AccountId).
/// Ссылка формируется как: https://t.me/{botName}?start={Key}
/// </summary>
public class ReferralLinkEntity
{
    /// <summary>Первичный ключ.</summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Короткий строковый ключ, включаемый в URL.
    /// Длина задаётся через переменную окружения KEY_LENGTH (по умолчанию 8 символов).
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>AccountId владельца ссылки. Unique index — у одного аккаунта не может быть двух ссылок.</summary>
    public Guid AccountId { get; set; }

    /// <summary>Дата создания ссылки. Устанавливается БД через CURRENT_TIMESTAMP.</summary>
    public DateTime CreatedDate { get; set; }
}
