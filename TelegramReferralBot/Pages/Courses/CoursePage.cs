using ReferralBot.Models;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Courses;

/// <summary>
/// Карточка отдельного курса.
/// В следующих итерациях можно наполнить реальными данными из Stepik API
/// через IConfiguration (SelectedCourseId из context).
/// </summary>
public class CoursePage(PageCreator pageCreator) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var text =
            """
            📚 КУРС

            Подробная информация о курсе доступна на платформе Stepik.
            Нажми кнопку ниже чтобы перейти к покупке со скидкой!
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
