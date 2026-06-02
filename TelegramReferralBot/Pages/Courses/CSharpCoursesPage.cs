using ReferralBot.Models;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Courses;

public class CSharpCoursesPage(PageCreator pageCreator) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var text =
            """
            🎓 КУРСЫ C# / .NET

            Выбери курс который тебя интересует:

            • C# с нуля — для начинающих
            • ASP.NET Core — backend-разработка
            • Алгоритмы и структуры данных

            Все курсы на платформе Stepik.
            При переходе по ссылке скидка применяется автоматически!
            """;

        return Task.FromResult(text);
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("C# с нуля"), pageCreator.CreatePage<CoursePage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ASP.NET Core"), pageCreator.CreatePage<CoursePage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("Назад ⬅️"), pageCreator.CreatePage<BackwardDummyPage>())]
        ]);
    }
}
