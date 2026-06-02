using Telegram.Bot;
using Telegram.Bot.Types;

namespace ReferralBot.Services.Bot;

public interface IBotService
{
    Task HandleUpdateAsync(Update update, ITelegramBotClient client, CancellationToken ct = default);
    Task HandleErrorAsync(Exception exception, CancellationToken ct = default);
}
