using ReferralBot.Core.Interfaces;
using ReferralBot.Models;
using ReferralBot.Pages;
using ReferralBot.Pages.Question;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ReferralBot.Services.Bot;

public class BotService(
    ILogger<BotService> logger,
    ITelegramBotUserStatesService statesService,
    ITelegramBotUserService userService,
    TelegramUserContextConverter contextConverter,
    CommandsProvider commandsProvider,
    PageCreator pageCreator,
    IConfiguration config) : IBotService
{
    public async Task HandleErrorAsync(Exception exception, CancellationToken ct = default)
    {
        var message = exception switch
        {
            ApiRequestException api => $"Telegram API Error [{api.ErrorCode}]: {api.Message}",
            _ => exception.ToString()
        };

        logger.LogError("Bot error: {Message}", message);
        await Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(Update update, ITelegramBotClient client, CancellationToken ct = default)
    {
        try
        {
            if (!IsValidUpdate(update))
            {
                logger.LogDebug("Skipped update type: {Type}", update.Type);
                return;
            }

            var telegramUserId = GetUserId(update);
            logger.LogInformation("Processing update {UpdateId} for user {UserId}", update.Id, telegramUserId);

            // Сразу показываем «печатает…»: пользователь видит, что бот работает, пока
            // мы ходим в БД и Stepik. Индикатор держится ~5с или до отправки ответа.
            await SendTypingActionAsync(client, update, ct);

            // 1. Загружаем состояние пользователя из БД
            var state = await statesService.GetByTelegramUserIdAsync(telegramUserId, ct);
            var context = contextConverter.ToContext(state);

            // 2. Новый пользователь — создаём контекст с начальной страницей
            if (context.TelegramId == 0)
            {
                context = CreateNewContext(telegramUserId, update);
                await statesService.AddOrUpdateAsync(contextConverter.ToState(context), ct);
            }

            // 3. GetOrCreate профиль Telegram-пользователя
            await GetOrCreateTelegramUser(telegramUserId, update, ct);

            // 4. Обрабатываем команды и текстовые сообщения
            if (update.Message is not null)
            {
                if (!ShouldProcessMessage(update, context))
                {
                    logger.LogDebug("Ignoring non-command text message for user {UserId}", telegramUserId);
                    return;
                }

                await commandsProvider.TryHandleCommandAsync(update, context, client, ct);

                // Если пользователь на странице вопроса и написал текст — пересылаем администратору
                if (context.CurrentPage is AskQuestionPage && !string.IsNullOrEmpty(update.Message.Text))
                    await ForwardQuestionToAdminAsync(update, context, client, ct);
            }

            // 5. Обрабатываем текущую страницу
            var result = await context.CurrentPage.HandleAsync(update, context);

            // 6. Навигация: добавляем новую страницу в стек если нужно
            if (result.NextPage is not null
                && result.NextPage != context.CurrentPage
                && result.NextPage is not BackwardDummyPage)
            {
                context.AddPage(result.NextPage);
            }

            // 7. Удаляем предыдущее сообщение бота (чистый UI при нажатии кнопок)
            await DeletePreviousMessageAsync(client, update, context, ct);

            // 8. Отправляем новое сообщение
            var sentMessage = await SendResultAsync(client, update, telegramUserId, result, ct);

            // 9. Сохраняем обновлённое состояние
            if (sentMessage is not null)
            {
                context.LastMessage = new TelegramBotMessageDto(sentMessage.MessageId, result.IsMedia);
                context.ActionsHistory.Add($"Sent {sentMessage.MessageId} ({result.GetType().Name})");
            }

            await statesService.AddOrUpdateAsync(contextConverter.ToState(context), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing update {UpdateId}", update.Id);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static bool IsValidUpdate(Update update) =>
        update.Type is UpdateType.Message or UpdateType.CallbackQuery;

    private static long GetUserId(Update update) =>
        update.Type == UpdateType.Message
            ? update.Message!.From!.Id
            : update.CallbackQuery!.From.Id;

    /// <summary>
    /// Шлёт статус «печатает…» в чат, чтобы пользователь видел, что бот занят работой
    /// (запросы к Stepik/БД). Не критично: если не отправится — молча пропускаем.
    /// </summary>
    private async Task SendTypingActionAsync(ITelegramBotClient client, Update update, CancellationToken ct)
    {
        var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;
        if (chatId is null) return;

        try
        {
            await client.SendChatAction(chatId.Value, ChatAction.Typing, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not send chat action to chat {ChatId}", chatId);
        }
    }

    private TelegramUserContext CreateNewContext(long telegramUserId, Update update)
    {
        logger.LogInformation("Creating new context for user {UserId}", telegramUserId);

        var messageId = update.Message?.MessageId ?? 0;
        var isMedia = update.Message?.Photo?.Length > 0;

        return new TelegramUserContext
        {
            TelegramId = telegramUserId,
            LastMessage = new TelegramBotMessageDto(messageId, isMedia),
            Pages = new Stack<IPage>([pageCreator.CreatePage<StartPage>()])
        };
    }

    private async Task GetOrCreateTelegramUser(long telegramUserId, Update update, CancellationToken ct)
    {
        var existing = await userService.GetByTelegramUserIdAsync(telegramUserId, ct);
        if (existing is not null) return;

        var from = update.Type == UpdateType.Message
            ? update.Message!.From
            : update.CallbackQuery!.From;

        var user = new Core.Models.TelegramBotUser
        {
            Id = telegramUserId,
            Username = from?.Username,
            FirstName = from?.FirstName ?? string.Empty,
            LastName = from?.LastName ?? string.Empty,
            IsPartner = false
        };

        await userService.AddOrUpdateAsync(user, ct);
        logger.LogInformation("Created TelegramBotUser: {UserId}", telegramUserId);
    }

    private static bool ShouldProcessMessage(Update update, TelegramUserContext context)
    {
        if (update.Message?.Type != MessageType.Text) return false;

        var text = update.Message.Text?.Trim() ?? string.Empty;

        // Разрешаем команды (начинаются с /) и текст на странице вопроса
        return text.StartsWith('/') || context.CurrentPage is AskQuestionPage;
    }

    private async Task ForwardQuestionToAdminAsync(
        Update update,
        TelegramUserContext context,
        ITelegramBotClient client,
        CancellationToken ct)
    {
        try
        {
            var adminId = config.GetValue<long>("ADMIN_TELEGRAM_ID");
            var from = update.Message!.From!;
            var text = update.Message.Text;

            var notification =
                $"❓ Новый вопрос от пользователя:\n" +
                $"Имя: {from.FirstName} {from.LastName} (@{from.Username})\n" +
                $"Telegram ID: {from.Id}\n\n" +
                $"Вопрос:\n{text}";

            await client.SendMessage(adminId, notification, cancellationToken: ct);

            logger.LogInformation("Forwarded question from user {UserId} to admin", context.TelegramId);

            context.AddPage(pageCreator.CreatePage<QuestionSentPage>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to forward question from user {UserId}", context.TelegramId);
        }
    }

    private async Task DeletePreviousMessageAsync(
        ITelegramBotClient client,
        Update update,
        TelegramUserContext context,
        CancellationToken ct)
    {
        if (context.LastMessage is null || update.CallbackQuery is null) return;

        try
        {
            await client.DeleteMessage(
                chatId: update.CallbackQuery.Message!.Chat.Id,
                messageId: context.LastMessage.TelegramMessageId,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            // Не критично — сообщение могло быть уже удалено
            logger.LogWarning(ex, "Could not delete message {MessageId}", context.LastMessage.TelegramMessageId);
        }
    }

    private async Task<Message?> SendResultAsync(
        ITelegramBotClient client,
        Update update,
        long userId,
        Pages.PageResults.PageResultBase result,
        CancellationToken ct)
    {
        var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;
        if (chatId is null)
        {
            logger.LogError("Cannot determine ChatId for user {UserId}", userId);
            return null;
        }

        try
        {
            if (result is Pages.PageResults.PhotoPageResult photo)
            {
                return await client.SendPhoto(
                    chatId: chatId.Value,
                    photo: photo.Photo,
                    caption: photo.Text,
                    parseMode: photo.ParseMode,
                    replyMarkup: photo.ReplyMarkup,
                    cancellationToken: ct);
            }

            return await client.SendMessage(
                chatId: chatId.Value,
                text: result.Text,
                parseMode: result.ParseMode,
                replyMarkup: result.ReplyMarkup,
                linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to user {UserId}", userId);
            return null;
        }
    }
}
