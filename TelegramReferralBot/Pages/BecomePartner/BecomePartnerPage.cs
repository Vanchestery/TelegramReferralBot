using ReferralBot.Models;
using ReferralBot.Pages.Partner;
using ReferralBot.Pages.Question;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.BecomePartner;

public class BecomePartnerPage(PageCreator pageCreator) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var text =
            """
            📋 УСЛОВИЯ ПАРТНЁРСКОЙ ПРОГРАММЫ

            1. Твоё согласие 😂
            2. ВСЁ!

            Что ты получишь:
            • 15–30% бонусных рублей с каждой покупки твоих рефералов
            • Пожизненные начисления от рефералов
            • Личный кабинет с подробной статистикой
            • Твои друзья получат скидку 10% на любой курс!

            Что надо делать:
            1. Рассказывать и рекомендовать наши курсы друзьям и знакомым.
            2. Давать им свою персональную ссылку, по которой они получат скидку, а тебе начислятся бонусные рубли.

            *Бонусный рубль = 1 рубль.
            *Реферал — твой друг, который перешёл по твоей персональной ссылке.
            *Пожизненные начисления — получение бонусных рублей от КАЖДОЙ покупки курса рефералом.
            """;

        return Task.FromResult(text);
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ГО ПАРТНЕРИТЬСЯ ✅"), pageCreator.CreatePage<WelcomePartnerPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ЗАДАТЬ ВОПРОС"), pageCreator.CreatePage<AskQuestionPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("Назад ⬅️"), pageCreator.CreatePage<BackwardDummyPage>())]
        ]);
    }
}
