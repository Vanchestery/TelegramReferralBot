using AutoMapper;

using Microsoft.Extensions.Logging;

using ReferralBot.Core.Interfaces;
using ReferralBot.Core.Models;
using ReferralBot.Db.Entities;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Core.Services;

public class ReferralLinkService(
    IReferralLinksStorage referralLinksStorage,
    IAccountsStorage accountsStorage,
    IMapper mapper,
    ILogger<ReferralLinkService> logger) : IReferralLinkService
{
    public async Task<ReferralLink> GetOrCreateAsync(long telegramUserId, CancellationToken ct = default)
    {
        logger.LogDebug("GetOrCreate ReferralLink for TelegramUserId: {Id}", telegramUserId);

        var account = await accountsStorage.GetByTelegramUserIdAsync(telegramUserId, ct)
            ?? throw new InvalidOperationException($"Account not found for TelegramUserId: {telegramUserId}");

        var existing = await referralLinksStorage.GetByAccountIdAsync(account.Id, ct);
        if (existing is not null)
            return mapper.Map<ReferralLink>(existing);

        // Генерируем уникальный ключ заданной длины
        var keyLength = int.TryParse(Environment.GetEnvironmentVariable("KEY_LENGTH"), out var len) ? len : 8;
        var key = GenerateKey(keyLength);

        var entity = new ReferralLinkEntity
        {
            Id = Guid.NewGuid(),
            Key = key,
            AccountId = account.Id,
            CreatedDate = DateTime.UtcNow
        };

        await referralLinksStorage.AddAsync(entity, ct);
        logger.LogInformation("Created ReferralLink for AccountId: {AccountId}, Key: {Key}", account.Id, key);

        return mapper.Map<ReferralLink>(entity);
    }

    public async Task<ReferralLink?> CheckSecretKeyAsync(string key, CancellationToken ct = default)
    {
        logger.LogDebug("Checking secret key: {Key}", key);

        var entity = await referralLinksStorage.GetByKeyAsync(key, ct);

        if (entity is null)
        {
            logger.LogWarning("ReferralLink not found for key: {Key}", key);
            return null;
        }

        return mapper.Map<ReferralLink>(entity);
    }

    private static string GenerateKey(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }
}
