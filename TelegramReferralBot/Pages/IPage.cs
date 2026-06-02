using ReferralBot.Models;
using ReferralBot.Pages.PageResults;

using Telegram.Bot.Types;

namespace ReferralBot.Pages;

/// <summary>
/// Интерфейс страницы бота.
/// Каждый экран в боте реализует этот интерфейс.
///
/// ViewAsync  — отрендерить страницу (первый показ или обновление).
/// HandleAsync — обработать действие пользователя на этой странице (нажатие кнопки).
/// </summary>
public interface IPage
{
    Task<PageResultBase> ViewAsync(Update update, TelegramUserContext context);
    Task<PageResultBase> HandleAsync(Update update, TelegramUserContext context);
}
