using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages;

/// <summary>
/// Связывает InlineKeyboardButton с целевой страницей.
/// Используется в GetKeyboardAsync для декларативного описания кнопок.
///
/// Пример:
///   new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("Кабинет"), pageCreator.CreatePage&lt;PartnerHomePage&gt;())
/// </summary>
public class ButtonLinqPage
{
    public InlineKeyboardButton Button { get; }

    /// <summary>
    /// Целевая страница для навигации по нажатию (callback-кнопка).
    /// null — у URL-кнопки: Telegram открывает ссылку сам, callback в бота не приходит,
    /// поэтому переходить внутри бота некуда.
    /// </summary>
    public IPage? Page { get; }

    public ButtonLinqPage(InlineKeyboardButton button, IPage page)
    {
        Button = button;
        Page = page;
    }

    /// <summary>Конструктор для URL-кнопки (ссылка наружу, без внутренней навигации).</summary>
    public ButtonLinqPage(InlineKeyboardButton button)
    {
        Button = button;
        Page = null;
    }
}
