using ReferralBot.Models;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages;

public class NotStartedPage(PageCreator pageCreator) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
        => Task.FromResult("Этот раздел ещё в разработке. Скоро здесь будет кое-что интересное!");

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("Назад ⬅️"), pageCreator.CreatePage<BackwardDummyPage>())]
        ]);
    }
}
