using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.PageResults;

/// <summary>
/// Результат страницы с документом. Отправляется через SendDocument.
/// </summary>
public class DocumentPageResult(InputFile document, string caption, InlineKeyboardMarkup replyMarkup)
    : PageResultBase(caption, replyMarkup)
{
    public InputFile Document { get; } = document;
}
