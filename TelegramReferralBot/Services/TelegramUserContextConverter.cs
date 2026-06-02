using ReferralBot.Core.Models;
using ReferralBot.Models;
using ReferralBot.Pages;

namespace ReferralBot.Services;

/// <summary>
/// Конвертирует между TelegramUserContext (in-memory объект бота)
/// и TelegramBotUserState (доменная модель для хранения в БД).
///
/// Вызывается в BotService при каждом update:
///   загрузка: State (DB) → Context (memory)
///   сохранение: Context (memory) → State (DB)
/// </summary>
public class TelegramUserContextConverter(
    PageStackConverter pageStackConverter,
    PageCreator pageCreator,
    ILogger<TelegramUserContextConverter> logger)
{
    public TelegramUserContext ToContext(TelegramBotUserState? state)
    {
        if (state is null)
            return new TelegramUserContext { TelegramId = 0 };

        try
        {
            return new TelegramUserContext
            {
                TelegramId = state.TelegramUserId,
                Pages = pageStackConverter.ToStack(state.PageNames),
                LastMessage = new TelegramBotMessageDto(state.CurrentMessageId, state.IsMediaContent),
                IsWelcomeMessageSent = state.IsWelcomeMessageSent
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to convert state to context for TelegramUserId: {Id}", state.TelegramUserId);
            return new TelegramUserContext { TelegramId = state.TelegramUserId };
        }
    }

    public TelegramBotUserState ToState(TelegramUserContext context)
    {
        return new TelegramBotUserState
        {
            TelegramUserId = context.TelegramId,
            PageNames = pageStackConverter.ToIds(context.Pages),
            CurrentMessageId = context.LastMessage?.TelegramMessageId ?? 0,
            IsMediaContent = context.LastMessage?.IsMedia ?? false,
            IsWelcomeMessageSent = context.IsWelcomeMessageSent
        };
    }
}
