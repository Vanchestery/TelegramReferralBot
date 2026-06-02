using System.Text.RegularExpressions;

using ReferralBot.Models;
using ReferralBot.Pages.PageResults;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Pages;

/// <summary>
/// Базовый класс для всех страниц бота с inline-кнопками.
///
/// Реализует шаблонный метод:
///   1. GetRawContentAsync  — текст страницы (переопределяется в наследнике)
///   2. GetKeyboardAsync     — матрица кнопок (переопределяется в наследнике)
///   3. GetMediaContentAsync — опциональное фото (переопределяется при необходимости)
///
/// ViewAsync и HandleAsync реализованы здесь — наследникам не нужно их трогать.
/// </summary>
public abstract class CallbackQueryPageBase : IPage
{
    /// <summary>Текст страницы до экранирования MarkdownV2.</summary>
    protected abstract Task<string> GetRawContentAsync(TelegramUserContext context);

    /// <summary>
    /// Матрица кнопок. Внешний массив — строки, внутренний — кнопки в строке.
    /// ButtonLinqPage связывает кнопку с целевой страницей.
    /// </summary>
    public abstract Task<ButtonLinqPage[][]> GetKeyboardAsync(TelegramUserContext context);

    /// <summary>Опциональное медиа для страницы. По умолчанию — null (текстовая страница).</summary>
    protected virtual Task<InputFile?> GetMediaContentAsync(TelegramUserContext context)
        => Task.FromResult<InputFile?>(null);

    public virtual async Task<PageResultBase> ViewAsync(Update update, TelegramUserContext context)
    {
        var text = await GetRawContentAsync(context);
        var media = await GetMediaContentAsync(context);
        var replyMarkup = await BuildKeyboardAsync(context);
        var escapedText = EscapeMarkdownV2(text);

        if (media is not null)
        {
            return new PhotoPageResult(media, escapedText, replyMarkup)
            {
                TelegramUserContext = context
            };
        }

        return new PageResultBase(escapedText, replyMarkup)
        {
            TelegramUserContext = context
        };
    }

    public virtual async Task<PageResultBase> HandleAsync(Update update, TelegramUserContext context)
    {
        if (update.CallbackQuery?.Data is not string callbackData)
            return await ViewAsync(update, context);

        var keyboard = await GetKeyboardAsync(context);
        var button = keyboard
            .SelectMany(row => row)
            .FirstOrDefault(b => b.Button.CallbackData == callbackData);

        if (button is null)
            return await ViewAsync(update, context);

        var nextPageResult = await button.Page.ViewAsync(update, context);

        return new PageResultBase(
            text: nextPageResult.Text,
            replyMarkup: nextPageResult.ReplyMarkup,
            nextPage: nextPageResult.NextPage ?? button.Page
        );
    }

    private async Task<InlineKeyboardMarkup> BuildKeyboardAsync(TelegramUserContext context)
    {
        var rows = await GetKeyboardAsync(context);
        return new InlineKeyboardMarkup(
            rows.Select(row => row.Select(b => b.Button).ToArray()).ToArray()
        );
    }

    /// <summary>
    /// Экранирует спецсимволы MarkdownV2.
    /// Telegram требует экранировать: _ * [ ] ( ) ~ ` > # + - = | { } . !
    /// </summary>
    private static string EscapeMarkdownV2(string text)
    {
        // Используем regex вместо ручного перебора — надёжнее и читаемее
        return Regex.Replace(text, @"([_*\[\]()~`>#+\-=|{}.!\\])", @"\$1");
    }
}
