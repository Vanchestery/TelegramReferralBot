using AutoMapper;

using Microsoft.Extensions.Logging;

using ReferralBot.Core.Interfaces;
using ReferralBot.Core.Models;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Core.Services;

public class AccountService(
    IAccountsStorage storage,
    IMapper mapper,
    ILogger<AccountService> logger) : IAccountService
{
    public async Task<Account?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting Account by TelegramUserId: {Id}", telegramUserId);

        var entity = await storage.GetByTelegramUserIdAsync(telegramUserId, ct);
        return entity is null ? null : mapper.Map<Account>(entity);
    }

    public async Task<Account> GetOrCreateAsync(long telegramUserId, CancellationToken ct = default)
    {
        logger.LogDebug("GetOrCreate Account for TelegramUserId: {Id}", telegramUserId);

        var entity = await storage.GetByTelegramUserIdAsync(telegramUserId, ct);

        if (entity is not null)
            return mapper.Map<Account>(entity);

        var newEntity = new AccountEntity
        {
            Id = Guid.NewGuid(),
            TelegramUserId = telegramUserId,
            BonusBalance = 0,
            IsPartner = false,
            Status = UserDbStatus.Active,
            InvitedPurchasesCount = 0,
            TotalBonusEarned = 0,
            CreatedDate = DateTime.UtcNow
        };

        await storage.UpsertAsync(newEntity, ct);
        logger.LogInformation("Created new Account for TelegramUserId: {Id}", telegramUserId);

        return mapper.Map<Account>(newEntity);
    }

    public async Task AddOrUpdateAsync(Account account, CancellationToken ct = default)
    {
        logger.LogDebug("AddOrUpdate Account: {Id}", account.Id);

        var entity = mapper.Map<AccountEntity>(account);
        await storage.UpsertAsync(entity, ct);
    }

    public async Task<bool> AddReferrerIdByTelegramIdAsync(long telegramUserId, Guid referrerAccountId, CancellationToken ct = default)
    {
        logger.LogDebug("Adding ReferrerId for TelegramUserId: {Id}", telegramUserId);

        var entity = await storage.GetByTelegramUserIdAsync(telegramUserId, ct);

        if (entity is null)
        {
            logger.LogWarning("Account not found for TelegramUserId: {Id}", telegramUserId);
            return false;
        }

        // Не перезаписываем реферера если уже установлен
        if (entity.ReferrerId is not null)
        {
            logger.LogInformation("ReferrerId already set for TelegramUserId: {Id}, skipping", telegramUserId);
            return false;
        }

        entity.ReferrerId = referrerAccountId;
        await storage.UpsertAsync(entity, ct);

        logger.LogInformation("Set ReferrerId for TelegramUserId: {Id}", telegramUserId);
        return true;
    }

    public async Task<bool> IsUserReferredAsync(long telegramUserId, CancellationToken ct = default)
    {
        return await storage.IsUserReferredAsync(telegramUserId, ct);
    }
}
