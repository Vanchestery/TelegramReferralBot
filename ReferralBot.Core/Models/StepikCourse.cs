namespace ReferralBot.Core.Models;

public class StepikCourse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    /// <summary>Числовая цена строкой ("68900.00"). Парсим в decimal при необходимости.</summary>
    public string? Price { get; set; }

    /// <summary>URL обложки курса (cover) на CDN Stepik.</summary>
    public string? Cover { get; set; }

    /// <summary>Цена для показа, уже с валютой ("68900 ₽"). null — бесплатный/нет цены.</summary>
    public string? DisplayPrice { get; set; }

    /// <summary>Порядок курса в выдаче преподавателя — для стабильной сортировки.</summary>
    public int Position { get; set; }

    // Флаги публикации — фильтруем, чтобы в бот не попали черновики/архив.
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
    public bool IsArchived { get; set; }
}
