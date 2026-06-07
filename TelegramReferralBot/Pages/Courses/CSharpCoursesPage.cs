using ReferralBot.Models;
using ReferralBot.Pages.PageResults;
using ReferralBot.Services;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Courses;

public class CSharpCoursesPage(
    PageCreator pageCreator,
    ICourseService courseService) : CallbackQueryPageBase
{
    protected override Task<string> GetRawContentAsync(TelegramUserContext context)
        => Task.FromResult("Выберите интересующий курс:");

    public override async Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        var courses = await courseService.GetCoursesIdTitleAsync();

        // Кнопка на каждый курс: подпись — название, CallbackData — id курса.
        var rows = courses
            .Select(c => new[]
            {
                new ButtonLinqPage(
                    InlineKeyboardButton.WithCallbackData(c.Title, c.Id.ToString()),
                    pageCreator.CreatePage<CoursePage>())
            })
            .ToList();

        rows.Add([
            new ButtonLinqPage(
                InlineKeyboardButton.WithCallbackData("Назад ⬅️"),
                pageCreator.CreatePage<BackwardDummyPage>())
        ]);

        return [.. rows];
    }

    /// <summary>
    /// Клик по курсу: его id приходит в CallbackData. Сохраняем выбор в контекст,
    /// кладём карточку курса в стек вручную и возвращаем её результат напрямую —
    /// так сохраняется медиа (PhotoPageResult с обложкой), которое базовый HandleAsync
    /// потерял бы, перепаковав в обычный PageResultBase.
    /// «Назад» (нечисловой CallbackData) уходит в базовую обработку.
    /// </summary>
    public override async Task<PageResultBase> HandleAsync(Update update, TelegramUserContext context)
    {
        if (update.CallbackQuery?.Data is not string data || !int.TryParse(data, out var courseId))
            return await base.HandleAsync(update, context);

        context.SelectedCourseId = courseId;

        var coursePage = pageCreator.CreatePage<CoursePage>();
        context.AddPage(coursePage);
        return await coursePage.ViewAsync(update, context);
    }
}
