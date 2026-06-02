using ReferralBot.Models;
using ReferralBot.Pages.PageResults;

using Telegram.Bot.Types;

namespace ReferralBot.Pages;

/// <summary>
/// Служебная страница — реализует кнопку «Назад».
/// При вызове снимает верхнюю страницу со стека и рендерит новую вершину.
///
/// Не регистрируется в PageNames — используется только как маркер навигации.
/// </summary>
public class BackwardDummyPage : IPage
{
    public async Task<PageResultBase> ViewAsync(Update update, TelegramUserContext context)
    {
        if (context.Pages.Count > 1)
            context.Pages.Pop();

        return await context.CurrentPage.ViewAsync(update, context);
    }

    public async Task<PageResultBase> HandleAsync(Update update, TelegramUserContext context)
        => await ViewAsync(update, context);
}
