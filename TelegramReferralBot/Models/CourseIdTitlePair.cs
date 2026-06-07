namespace ReferralBot.Models;

/// <summary>
/// Краткая запись о курсе для списка: идентификатор + название.
/// Полная детализация подтягивается отдельно по Id при открытии карточки.
/// </summary>
public record CourseIdTitlePair
{
    public int Id { get; init; }
    public required string Title { get; init; }
}
