namespace ReferralBot.Models;

/// <summary>
/// Детальная карточка курса для отображения в боте: название, краткое описание и цена.
/// </summary>
public record CourseDetails
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;

    /// <summary>Цена в базовой валюте. 0 — цена не указана/бесплатно.</summary>
    public decimal Price { get; init; }
}
