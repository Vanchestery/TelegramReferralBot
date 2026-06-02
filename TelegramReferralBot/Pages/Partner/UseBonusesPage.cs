using ReferralBot.Core.Interfaces;
using ReferralBot.Models;
using ReferralBot.Pages.Courses;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Partner;

public class UseBonusesPage(
    PageCreator pageCreator,
    IPartnerService partnerService,
    ILogger<UseBonusesPage> logger) : CallbackQueryPageBase
{
    protected override async Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var profile = await partnerService.GetProfileAsync(context.TelegramId);

        if (profile is null)
        {
            logger.LogWarning("Partner profile not found for TelegramId: {Id}", context.TelegramId);
            return "Не удалось загрузить информацию о бонусах. Попробуйте позже.";
        }

        return $"""
                🎁 ИСПОЛЬЗОВАТЬ БОНУСЫ

                Доступно бонусов: {profile.BonusBalance} (= {profile.BonusBalance}₽)

                Куда направить бонусы?

                [🎓 Купить курс]
                └ Скидка до 50% стоимости

                [🏪 Купить мерч]
                ├ Футболки, худи, стикеры
                └ Скидка до 100% бонусами
                """;
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("🎓 КУПИТЬ КУРС"), pageCreator.CreatePage<CSharpCoursesPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("🏪 КУПИТЬ МЕРЧ"), pageCreator.CreatePage<IronStorePage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("Назад ⬅️"), pageCreator.CreatePage<BackwardDummyPage>())]
        ]);
    }
}
