using ReferralBot.Models;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Question;

public class QuestionSentPage(PageCreator pageCreator) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var text =
            """
            ✅ ВОПРОС ОТПРАВЛЕН

            Ваш вопрос успешно передан администратору.
            Ожидайте ответа — обычно это занимает не более 24 часов.
            """;

        return Task.FromResult(text);
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("На главную"), pageCreator.CreatePage<BackwardDummyPage>())]
        ]);
    }
}
