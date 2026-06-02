using ReferralBot.Core.Interfaces;
using ReferralBot.Models;
using ReferralBot.Pages.BecomePartner;
using ReferralBot.Pages.Courses;
using ReferralBot.Pages.Question;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Referral;

/// <summary>
/// Главная страница для пользователей, пришедших по реферальной ссылке.
/// Персонализирована — показывает имя партнёра, который пригласил.
/// </summary>
public class ReferralStartPage(
    PageCreator pageCreator,
    IPartnerService partnerService) : CallbackQueryPageBase
{
    protected override async Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var referrerName = await partnerService.GetReferrerNameByTelegramIdAsync(context.TelegramId)
                           ?? "нашего партнёра";

        return $"""
                Привет.

                Ты сюда попал по ссылке от {referrerName} и благодаря этому ты теперь ВСЕГДА можешь приобрести любой курс со скидкой 10%! Жми на кнопку [ВЫБРАТЬ КУРС].

                Как только ты познакомишься с нашими курсами у тебя появится дикое желание рекомендовать их. А за рекомендации можно получить разные плюшки. Жми на кнопку [ХОЧУ ПЛЮШКИ].

                Познакомиться с нами можно по кнопке [О ШКОЛЕ].
                """;
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ВЫБРАТЬ КУРС"), pageCreator.CreatePage<CSharpCoursesPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ХОЧУ ПЛЮШКИ"), pageCreator.CreatePage<BecomePartnerPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("О ШКОЛЕ"), pageCreator.CreatePage<AboutTheSchoolPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ЗАДАТЬ ВОПРОС"), pageCreator.CreatePage<AskQuestionPage>())]
        ]);
    }
}
