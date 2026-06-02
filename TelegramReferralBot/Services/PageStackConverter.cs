using ReferralBot.Pages;

namespace ReferralBot.Services;

/// <summary>
/// Конвертирует стек страниц в список строк (для хранения в БД) и обратно.
///
/// Формат хранения: List&lt;string&gt; с полными именами типов (Type.FullName).
/// Порядок: первый элемент = дно стека, последний = вершина.
/// </summary>
public class PageStackConverter(PagesFactory pagesFactory, ILogger<PageStackConverter> logger)
{
    /// <summary>Stack → List&lt;string&gt; (для сохранения в БД).</summary>
    public List<string> ToIds(Stack<IPage> pages)
    {
        // Reverse() — стек хранит вершину первой, нам нужен порядок от дна к вершине
        return pages.Reverse()
            .Select(p => p.GetType().FullName
                ?? throw new InvalidOperationException($"Type {p.GetType()} has no FullName"))
            .ToList();
    }

    /// <summary>List&lt;string&gt; → Stack (при загрузке из БД).</summary>
    public Stack<IPage> ToStack(List<string> pageIds)
    {
        if (pageIds.Count == 0)
            return new Stack<IPage>();

        try
        {
            // new Stack<IPage>(IEnumerable) кладёт последний элемент на вершину —
            // именно то что нам нужно при порядке "от дна к вершине"
            var pages = pageIds.Select(id => pagesFactory.GetPage(id));
            return new Stack<IPage>(pages);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to convert page IDs to stack, returning empty stack");
            return new Stack<IPage>();
        }
    }
}
