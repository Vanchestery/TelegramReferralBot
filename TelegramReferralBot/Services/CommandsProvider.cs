using ReferralBot.Core.Interfaces;
using ReferralBot.Models;
using ReferralBot.Pages;
using ReferralBot.Pages.Referral;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ReferralBot.Services;

public class CommandsProvider(
    IAccountService accountService,
    IReferralLinkService referralLinkService,
    IWelcomeVideoService welcomeVideoService,
    PageCreator pageCreator,
    IConfiguration config,
    ILogger<CommandsProvider> logger)
{
    public async Task SetCommandsAsync(ITelegramBotClient client, CancellationToken ct = default)
    {
        var commands = new[]
        {
            new BotCommand { Command = "start", Description = "Начать работу с ботом" }
        };

        await client.SetMyCommands(commands, cancellationToken: ct);
        logger.LogInformation("Bot commands registered");
    }

    public async Task TryHandleCommandAsync(
        Update update,
        TelegramUserContext context,
        ITelegramBotClient client,
        CancellationToken ct = default)
    {
        var text = update.Message?.Text?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        if (text.StartsWith("/start"))
            await HandleStartCommandAsync(update, context, client, text, ct);
    }

    private async Task HandleStartCommandAsync(
        Update update,
        TelegramUserContext context,
        ITelegramBotClient client,
        string messageText,
        CancellationToken ct)
    {
        var telegramUserId = context.TelegramId;

        // Если пользователь уже был рефералом — сбрасываем контекст (защита от повторного /start)
        var isReferred = await accountService.IsUserReferredAsync(telegramUserId, ct);
        if (isReferred)
            ResetContext(update, context);

        // Проверяем наличие реферального ключа в команде: /start {key}
        var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var keyLength = config.GetValue<int>("KEY_LENGTH", 8);

        if (parts.Length > 1 && parts[1].Length == keyLength)
            await HandleReferralKeyAsync(context, parts[1], ct);
        else
            ResetContext(update, context);

        // Отправляем приветственное видео один раз
        if (!context.IsWelcomeMessageSent)
        {
            var sent = await SendWelcomeVideoAsync(client, update.Message!.Chat.Id, ct);
            if (sent) context.IsWelcomeMessageSent = true;
        }
    }

    private void ResetContext(Update update, TelegramUserContext context)
    {
        context.TelegramId = update.Message!.From!.Id;

        if (context.Pages.Count > 1)
            context.ResetPages();

        if (context.Pages.Count == 0)
            context.AddPage(pageCreator.CreatePage<StartPage>());

        logger.LogDebug("Context reset for user {UserId}", context.TelegramId);
    }

    private async Task HandleReferralKeyAsync(TelegramUserContext context, string key, CancellationToken ct)
    {
        try
        {
            var referralLink = await referralLinkService.CheckSecretKeyAsync(key, ct);
            if (referralLink is null)
            {
                logger.LogWarning("Invalid referral key: {Key}", key);
                return;
            }

            await accountService.GetOrCreateAsync(context.TelegramId, ct);
            var added = await accountService.AddReferrerIdByTelegramIdAsync(
                context.TelegramId, referralLink.AccountId, ct);

            if (added)
            {
                context.AddPage(pageCreator.CreatePage<ReferralStartPage>());
                logger.LogInformation("User {UserId} registered via referral key {Key}", context.TelegramId, key);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing referral key {Key} for user {UserId}", key, context.TelegramId);
        }
    }

    private async Task<bool> SendWelcomeVideoAsync(ITelegramBotClient client, long chatId, CancellationToken ct)
    {
        try
        {
            var fileId = await welcomeVideoService.GetActiveFileIdAsync(ct);
            if (fileId is null)
            {
                logger.LogWarning("No active welcome video found");
                return false;
            }

            await client.SendVideo(
                chatId: chatId,
                video: new InputFileId(fileId),
                caption: "Добро пожаловать в реферальную программу школы IRON PROGRAMMER!",
                cancellationToken: ct);

            logger.LogInformation("Welcome video sent to chat {ChatId}", chatId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send welcome video to chat {ChatId}", chatId);
            return false;
        }
    }
}
