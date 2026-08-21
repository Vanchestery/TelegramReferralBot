using ReferralBot.Core.Interfaces;
using ReferralBot.Core.Models;
using ReferralBot.Models;
using ReferralBot.Pages.Courses;
using ReferralBot.Pages.Question;
using ReferralBot.Services;

using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Partner;

public class PartnerHomePage(
    PageCreator pageCreator,
    IPartnerService partnerService,
    IAccountService accountService,
    ITelegramBotUserService telegramBotUserService,
    IReferralLinkService referralLinkService,
    IConfiguration config,
    ILogger<PartnerHomePage> logger) : CallbackQueryPageBase
{
    protected override async Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        await telegramBotUserService.UpdatePartnerStatusAsync(context.TelegramId, true);
        await accountService.GetOrCreateAsync(context.TelegramId);

        var profile = await partnerService.GetProfileAsync(context.TelegramId);
        if (profile is null)
        {
            logger.LogWarning("Partner profile not found for TelegramId: {Id}", context.TelegramId);
            return "Не удалось загрузить кабинет партнёра. Попробуйте позже.";
        }

        var referralLink = await referralLinkService.GetOrCreateAsync(context.TelegramId);
        var botName = config["BOT_USERNAME"];
        var link = $"https://t.me/{botName}?start={referralLink.Key}";

        var levelName = FormatLevel(profile.Level);
        var nextLevelInfo = GetNextLevelProgress(profile);
        var levelPath = BuildLevelPath(profile.Level);

        return $"""
                💼 КАБИНЕТ ПАРТНЁРА

                Статус: {levelName}
                Процент: {profile.BonusRate}%

                💰 Баланс: {profile.BonusBalance} бонусов (= {profile.BonusBalance}₽)

                🎯 До следующего уровня:
                {nextLevelInfo}

                {levelPath}

                📈 СТАТИСТИКА:
                👥 Приглашено: {FormatPeople(profile.InvitedCount)}
                ✅ Купили курс: {FormatPeople(profile.InvitedPurchasesCount)}
                💵 Общий доход: {profile.TotalBonusEarned}₽

                🔗 Твоя реферальная ссылка:
                {link}
                """;
    }

    public override Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        return Task.FromResult<ButtonLinqPage[][]>(
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ВЫБРАТЬ КУРС"), pageCreator.CreatePage<CSharpCoursesPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("🎁 ИСПОЛЬЗОВАТЬ БОНУСЫ"), pageCreator.CreatePage<UseBonusesPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("📊 ДЕТАЛЬНАЯ СТАТИСТИКА"), pageCreator.CreatePage<StatisticsPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("О ШКОЛЕ"), pageCreator.CreatePage<AboutTheSchoolPage>())],
            [new ButtonLinqPage(InlineKeyboardButton.WithCallbackData("ЗАДАТЬ ВОПРОС"), pageCreator.CreatePage<AskQuestionPage>())]
        ]);
    }

    private static string FormatLevel(UserLevel level) => level switch
    {
        UserLevel.Intern => "📱 СТАЖЁР",
        UserLevel.Junior => "💻 ДЖУН",
        UserLevel.Middle => "⚡ МИДЛ",
        UserLevel.Senior => "🚀 СЕНЬОР",
        UserLevel.Ambassador => "💎 АМБАССАДОР",
        _ => "НЕИЗВЕСТНО"
    };

    private static string GetNextLevelProgress(PartnerProfile profile) =>
        profile.Level switch
        {
            UserLevel.Intern => $"Приведи ещё {FormatReferrals(3 - profile.InvitedCount)} → 💻 ДЖУН (20%)",
            UserLevel.Junior => $"Приведи ещё {FormatReferrals(6 - profile.InvitedCount)} → ⚡ МИДЛ (25%)",
            UserLevel.Middle => $"Приведи ещё {FormatReferrals(11 - profile.InvitedCount)} → 🚀 СЕНЬОР (27%)",
            UserLevel.Senior => $"Приведи ещё {FormatReferrals(21 - profile.InvitedCount)} → 💎 АМБАССАДОР (30%)",
            UserLevel.Ambassador => "Вы достигли максимального уровня!",
            _ => "Прогресс неизвестен"
        };

    private static string BuildLevelPath(UserLevel current)
    {
        var levels = new (UserLevel Level, string Name)[]
        {
            (UserLevel.Intern, "Стажёр"),
            (UserLevel.Junior, "Джун"),
            (UserLevel.Middle, "Мидл"),
            (UserLevel.Senior, "Сеньор"),
            (UserLevel.Ambassador, "Амбассадор")
        };

        var lines = new List<string> { "🏆 ТВОЙ ПУТЬ:" };
        foreach (var (level, name) in levels)
        {
            var icon = level <= current ? "✅" : "🔒";
            var marker = level == current ? " ← ты здесь" : "";
            lines.Add($"{icon} {name}{marker}");
        }
        return string.Join("\n", lines);
    }

    private static string FormatReferrals(int count)
    {
        if (count <= 0) return "0 рефералов";
        var last = count % 10;
        var lastTwo = count % 100;
        if (lastTwo is >= 11 and <= 14) return $"{count} рефералов";
        return last switch { 1 => $"{count} реферала", 2 or 3 or 4 => $"{count} реферала", _ => $"{count} рефералов" };
    }

    private static string FormatPeople(int count)
    {
        if (count == 0) return "0 людей";
        var last = count % 10;
        var lastTwo = count % 100;
        if (lastTwo is >= 11 and <= 14) return $"{count} людей";
        return last switch { 1 => $"{count} человек", 2 or 3 or 4 => $"{count} человека", _ => $"{count} людей" };
    }
}
