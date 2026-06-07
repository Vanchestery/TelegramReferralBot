using ReferralBot.Core.Interfaces;
using ReferralBot.Models;
using ReferralBot.Services;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages.Courses;

/// <summary>
/// Карточка курса: обложка (фото) + название/описание/цена + кнопки
/// «Посмотреть курс», «Купить со скидкой» (с персональным промокодом партнёра,
/// если он есть) и «Назад». Данные тянутся по context.SelectedCourseId.
/// </summary>
public class CoursePage(
    PageCreator pageCreator,
    ICourseService courseService,
    IPromoCodeService promoCodeService,
    IAccountService accountService,
    ITelegramBotClient botClient,
    ILogger<CoursePage> logger) : CallbackQueryPageBase
{
    protected override async Task<string> GetRawContentAsync(TelegramUserContext context)
    {
        var course = await courseService.GetCourseInfoAsync(context.SelectedCourseId);
        if (course is null)
            return "Не удалось загрузить информацию о курсе. Попробуйте позже.";

        // Подпись к фото ограничена 1024 символами — подрезаем длинное описание.
        var summary = course.Summary.Length > 600
            ? course.Summary[..600].TrimEnd() + "…"
            : course.Summary;

        var price = course.Price > 0
            ? $"{course.Price:N0} ₽"
            : "цена не указана";

        return $"{course.Title}\n\n{summary}\n\nЦена: {price}";
    }

    protected override async Task<InputFile?> GetMediaContentAsync(TelegramUserContext context)
    {
        // На карточке курса показываем «отправляет фото…», пока качаем обложку и шлём её
        // (точнее «печатает…», которое бот выставляет в начале обработки апдейта).
        try
        {
            await botClient.SendChatAction(context.TelegramId, ChatAction.UploadPhoto);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not send upload-photo action for course {CourseId}", context.SelectedCourseId);
        }

        var logo = await courseService.GetCourseLogoAsync(context.SelectedCourseId);
        return logo is { Length: > 0 }
            ? InputFile.FromStream(new MemoryStream(logo), $"course_{context.SelectedCourseId}.png")
            : null;
    }

    public override async Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context)
    {
        var courseId = context.SelectedCourseId;
        var courseUrl = $"https://stepik.org/a/{courseId}";

        // Персональная скидка партнёра, если в нашей БД есть его промокод на курс;
        // иначе — обычная ссылка оплаты без промо (graceful fallback).
        var account = await accountService.GetByTelegramUserIdAsync(context.TelegramId);
        var hex = account is null
            ? null
            : await promoCodeService.GetHexForPaymentAsync(courseId, account.Id);

        var payUrl = string.IsNullOrEmpty(hex)
            ? $"https://stepik.org/a/{courseId}/pay"
            : $"https://stepik.org/a/{courseId}/pay?promo={hex}";

        return
        [
            [new ButtonLinqPage(InlineKeyboardButton.WithUrl("Посмотреть курс", courseUrl))],
            [new ButtonLinqPage(InlineKeyboardButton.WithUrl("🛒 Купить со скидкой", payUrl))],
            [new ButtonLinqPage(
                InlineKeyboardButton.WithCallbackData("Назад ⬅️"),
                pageCreator.CreatePage<BackwardDummyPage>())],
        ];
    }
}
