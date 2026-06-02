using AutoMapper;

using Microsoft.Extensions.Logging;

using ReferralBot.Core.Interfaces;
using ReferralBot.Core.Models;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Core.Services;

public class TelegramBotUserStatesService(
    ITelegramUserStatesStorage storage,
    IMapper mapper,
    ILogger<TelegramBotUserStatesService> logger) : ITelegramBotUserStatesService
{
    public async Task<TelegramBotUserState?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting UserState for TelegramUserId: {Id}", telegramUserId);

        var entity = await storage.GetByTelegramUserIdAsync(telegramUserId, ct);
        return entity is null ? null : mapper.Map<TelegramBotUserState>(entity);
    }

    public async Task AddOrUpdateAsync(TelegramBotUserState state, CancellationToken ct = default)
    {
        logger.LogDebug("Saving UserState for TelegramUserId: {Id}", state.TelegramUserId);

        var entity = mapper.Map<TelegramBotUserStateEntity>(state);
        await storage.UpsertAsync(entity, ct);
    }
}
