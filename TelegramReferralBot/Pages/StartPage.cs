using ReferralBot.Models;
using ReferralBot.Pages.BecomePartner;
using ReferralBot.Pages.Courses;
using ReferralBot.Pages.Question;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages;

public class StartPage(PageCreator pageCreator) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var text =
            """
            Привет.

            Ты попал в партнёрскую программу школы IRON PROGRAMMER.

            У тебя есть возможность приобрести любой курс со скидкой 10%! Жми на кнопку [ВЫБРАТЬ КУРС].

            Как только ты притронешься к нашим курсам у тебя появится жуткое желание рекомендовать их. А за рекомендации можно получить разные плюшки. Жми на кнопку [ХОЧУ ПЛЮШКИ].

            Познакомиться с нами можно по кнопке [О ШКОЛЕ].
            """;

        return Task.FromResult(text);
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ВЫБРАТЬ КУРС"), pageCreator.CreatePage<CSharpCoursesPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ХОЧУ ПЛЮШКИ"), pageCreator.CreatePage<BonusBenefitsPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("О ШКОЛЕ"), pageCreator.CreatePage<AboutTheSchoolPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ЗАДАТЬ ВОПРОС"), pageCreator.CreatePage<AskQuestionPage>())]
        ]);
    }
}
