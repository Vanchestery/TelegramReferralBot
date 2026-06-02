using ReferralBot.Models;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Partner;

public class WelcomePartnerPage(PageCreator pageCreator) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var text =
            """
            🎉 Юху! Поздравляем, мы партнёры 😊

            У тебя появился личный кабинет! Там ты найдёшь:
            1. Персональную ссылку для приглашения друзей
            2. Баланс бонусных рублей
            3. Количество приглашённых друзей
            4. Плюшки

            Чем больше ты приводишь друзей, тем больше ты получаешь бонусных рублей.

            Как это работает:
            📱 СТАЖЁР (0–2 рефералов) — 15%
            💻 ДЖУН (3–5 рефералов) — 20%
            ⚡ МИДЛ (6–10 рефералов) — 25%
            🚀 СЕНЬОР (11–20 рефералов) — 27%
            💎 АМБАССАДОР (21+ рефералов) — 30%

            Например: если твой приглашённый друг заплатил за курс 5000 рублей, и у тебя статус ДЖУН, то ты получишь 20% = 1000 бонусных рублей.

            Причём со всех дальнейших покупок твоих рефералов ты также получишь бонусы согласно своему статусу.
            """;

        return Task.FromResult(text);
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("💼 КАБИНЕТ ПАРТНЁРА"), pageCreator.CreatePage<PartnerHomePage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ЗАДАТЬ ВОПРОС"), pageCreator.CreatePage<Question.AskQuestionPage>())]
        ]);
    }
}
