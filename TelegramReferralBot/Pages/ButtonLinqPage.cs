using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages;

/// <summary>
/// Связывает InlineKeyboardButton с целевой страницей.
/// Используется в GetKeyboardAsync для декларативного описания кнопок.
///
/// Пример:
///   new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("Кабинет"), pageCreator.CreatePage&lt;PartnerHomePage&gt;())
/// </summary>
public class ButtonLinqPage(InlineKeyboardButton button, IPage page)
{
    public InlineKeyboardButton Button { get; } = button;
    public IPage Page { get; } = page;
}
