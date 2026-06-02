using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.PageResults;

/// <summary>
/// Результат страницы с фото. Отправляется через SendPhoto вместо SendMessage.
/// </summary>
public class PhotoPageResult(InputFile photo, string caption, InlineKeyboardMarkup replyMarkup)
    : PageResultBase(caption, replyMarkup)
{
    public InputFile Photo { get; } = photo;
}
