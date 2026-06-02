using ReferralBot.Core.Interfaces;
using ReferralBot.Models;
using ReferralBot.Pages.Question;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Partner;

public class StatisticsPage(
    PageCreator pageCreator,
    IPartnerService partnerService,
    ILogger<StatisticsPage> logger) : CallbackQueryPageBase
{
    protected override async Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var profile = await partnerService.GetProfileAsync(context.TelegramId);

        if (profile is null)
        {
            logger.LogWarning("Partner profile not found for TelegramId: {Id}", context.TelegramId);
            return "Не удалось загрузить статистику. Попробуйте позже.";
        }

        var withdrawn = profile.TotalBonusEarned - profile.BonusBalance;

        return $"""
                📊 ДЕТАЛЬНАЯ СТАТИСТИКА

                🗓 За всё время:
                👥 Рефералов: {profile.InvitedCount}
                ✅ Куплено курсов рефералами: {profile.InvitedPurchasesCount}

                💵 ФИНАНСЫ:
                Всего заработано: {profile.TotalBonusEarned}₽
                ├ Потрачено: {withdrawn}₽
                └ Доступно: {profile.BonusBalance}₽
                """;
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ЗАДАТЬ ВОПРОС"), pageCreator.CreatePage<AskQuestionPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("Назад ⬅️"), pageCreator.CreatePage<BackwardDummyPage>())]
        ]);
    }
}
