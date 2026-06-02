using ReferralBot.Pages;

namespace ReferralBot.Models;

/// <summary>
/// Контекст взаимодействия пользователя с ботом.
/// Живёт в памяти в рамках одного запроса — персистируется через TelegramBotUserStatesService.
///
/// Стек Pages — основа навигации:
///   - вершина = текущая страница (CurrentPage)
///   - BackwardDummyPage делает Pop() → возвращает на предыдущую
///   - ResetPages() оставляет только дно стека (начальную страницу)
/// </summary>
public class TelegramUserContext
{
    public long TelegramId { get; set; }

    /// <summary>Стек страниц. Вершина = текущая страница.</summary>
    public Stack<IPage> Pages { get; set; } = new();

    /// <summary>Последнее сообщение бота — нужно для удаления перед отправкой нового.</summary>
    public TelegramBotMessageDto? LastMessage { get; set; }

    /// <summary>История действий для отладки.</summary>
    public List<string> ActionsHistory { get; set; } = [];

    /// <summary>Выбранный пользователем курс (для страниц курсов).</summary>
    public int SelectedCourseId { get; set; }

    /// <summary>Приветственное видео уже было отправлено.</summary>
    public bool IsWelcomeMessageSent { get; set; }

    /// <summary>Текущая страница — вершина стека.</summary>
    public IPage CurrentPage => Pages.Peek();

    /// <summary>
    /// Добавляет страницу в стек.
    /// Защита от дублей: не добавляет если тип совпадает с текущей страницей.
    /// </summary>
    public void AddPage(IPage page)
    {
        if (Pages.Count == 0 || CurrentPage.GetType() != page.GetType())
            Pages.Push(page);
    }

    /// <summary>
    /// Сбрасывает стек до начальной страницы (оставляет только дно).
    /// Вызывается при /start.
    /// </summary>
    public void ResetPages()
    {
        if (Pages.Count == 0) return;

        while (Pages.Count > 1)
            Pages.Pop();
    }
}
