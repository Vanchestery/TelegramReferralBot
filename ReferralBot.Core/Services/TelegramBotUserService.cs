using AutoMapper;

using Microsoft.Extensions.Logging;

using ReferralBot.Core.Interfaces;
using ReferralBot.Core.Models;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Core.Services;

public class TelegramBotUserService(
    ITelegramBotUsersStorage storage,
    IMapper mapper,
    ILogger<TelegramBotUserService> logger) : ITelegramBotUserService
{
    public async Task<TelegramBotUser?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting TelegramBotUser: {Id}", telegramUserId);

        var entity = await storage.GetByIdAsync(telegramUserId, ct);
        return entity is null ? null : mapper.Map<TelegramBotUser>(entity);
    }

    public async Task AddOrUpdateAsync(TelegramBotUser user, CancellationToken ct = default)
    {
        logger.LogDebug("Adding or updating TelegramBotUser: {Id}", user.Id);

        var entity = mapper.Map<TelegramBotUserEntity>(user);
        await storage.UpsertAsync(entity, ct);
    }

    public async Task UpdatePartnerStatusAsync(long telegramUserId, bool isPartner, CancellationToken ct = default)
    {
        logger.LogDebug("Updating partner status for TelegramUserId: {Id}", telegramUserId);

        await storage.UpdatePartnerStatusAsync(telegramUserId, isPartner, ct);
    }
}
