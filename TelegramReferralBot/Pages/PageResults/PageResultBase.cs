using ReferralBot.Models;
using ReferralBot.Pages;

using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.PageResults;

/// <summary>
/// Результат рендера страницы — содержит всё необходимое для отправки сообщения в Telegram.
/// В Telegram.Bot 22.x нет общего базового класса ReplyMarkup —
/// используем InlineKeyboardMarkup напрямую (все страницы бота используют inline-кнопки).
/// </summary>
public class PageResultBase
{
    public string Text { get; }
    public InlineKeyboardMarkup ReplyMarkup { get; }
    public ParseMode ParseMode { get; set; } = ParseMode.MarkdownV2;

    /// <summary>
    /// Следующая страница для навигации.
    /// Устанавливается в HandleAsync когда пользователь нажал кнопку.
    /// null — остаёмся на текущей странице.
    /// </summary>
    public IPage? NextPage { get; }

    /// <summary>true если результат содержит медиа (фото, документ).</summary>
    public bool IsMedia => this is PhotoPageResult or DocumentPageResult;

    /// <summary>Контекст пользователя — устанавливается в ViewAsync страницы.</summary>
    public TelegramUserContext TelegramUserContext { get; set; } = null!;

    public PageResultBase(string text, InlineKeyboardMarkup replyMarkup, IPage? nextPage = null)
    {
        Text = text;
        ReplyMarkup = replyMarkup;
        NextPage = nextPage;
    }
}
