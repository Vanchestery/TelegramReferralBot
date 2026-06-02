using ReferralBot.Models;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Question;

/// <summary>
/// Страница отправки вопроса администратору.
/// Когда пользователь находится на этой странице и пишет текст —
/// BotService перехватывает сообщение и пересылает администратору.
/// </summary>
public class AskQuestionPage(PageCreator pageCreator) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var text =
            """
            ❓ ЗАДАТЬ ВОПРОС

            Напишите ваш вопрос следующим сообщением — я передам его администратору.

            Постарайтесь описать вопрос подробно, чтобы получить точный ответ.
            """;

        return Task.FromResult(text);
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("Назад ⬅️"), pageCreator.CreatePage<BackwardDummyPage>())]
        ]);
    }
}
