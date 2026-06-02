namespace ReferralBot.Db.Entities;

/// <summary>
/// Приветственное видео, отправляемое новым пользователям.
/// Хранит Telegram file_id — после первой загрузки видео можно отправлять повторно
/// без повторной загрузки файла на сервер Telegram.
/// </summary>
public class WelcomeVideoEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Telegram file_id — получается после первой отправки файла боту.
    /// Позволяет переотправлять видео через SendVideo(new InputFileId(fileId)).
    /// </summary>
    public string FileId { get; set; } = string.Empty;

    /// <summary>Локальный путь к файлу на сервере для первичной загрузки.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Дата добавления записи.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Активная версия видео. Только одна запись должна быть IsActive=true.
    /// Остальные деактивируются при добавлении нового видео (soft replace).
    /// </summary>
    public bool IsActive { get; set; } = true;
}
