using ReferralBot.Models;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages;

public class AboutTheSchoolPage(PageCreator pageCreator) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var text =
            """
            🏫 О ШКОЛЕ IRON PROGRAMMER

            Мы обучаем программированию с нуля до трудоустройства.

            Наши курсы:
            • C# с нуля до Junior
            • ASP.NET Core — backend-разработка
            • Алгоритмы и структуры данных

            Все курсы доступны на платформе Stepik.
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
