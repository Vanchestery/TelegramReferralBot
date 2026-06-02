using ReferralBot.Models;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.BecomePartner;

public class BonusBenefitsPage(PageCreator pageCreator) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var text =
            """
            🎁 ПЛЮШКИ ПАРТНЁРСКОЙ ПРОГРАММЫ

            Став партнёром ты получаешь:

            • 15–30% бонусных рублей с каждой покупки твоих рефералов
            • Пожизненные начисления от рефералов
            • Личный кабинет с подробной статистикой
            • Твои друзья получат скидку 10% на любой курс!

            Бонусный рубль = 1 рубль.
            Тратить можно на курсы и мерч школы.

            Хочешь стать партнёром?
            """;

        return Task.FromResult(text);
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("СТАТЬ ПАРТНЁРОМ ✅"), pageCreator.CreatePage<BecomePartnerPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("Назад ⬅️"), pageCreator.CreatePage<BackwardDummyPage>())]
        ]);
    }
}
