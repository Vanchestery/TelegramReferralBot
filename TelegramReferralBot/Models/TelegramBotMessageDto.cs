namespace ReferralBot.Models;

/// <summary>
/// Данные последнего сообщения бота.
/// Нужны для удаления предыдущего сообщения перед отправкой нового (чистый UI).
/// </summary>
public record TelegramBotMessageDto(int TelegramMessageId, bool IsMedia);
