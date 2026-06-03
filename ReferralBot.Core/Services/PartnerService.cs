using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using ReferralBot.Core.Interfaces;
using ReferralBot.Core.Models;
using ReferralBot.Db.Interfaces;

namespace ReferralBot.Core.Services;

public class PartnerService(
    IAccountsStorage accountsStorage,
    ITelegramBotUsersStorage usersStorage,
    IMemoryCache cache,
    ILogger<PartnerService> logger) : IPartnerService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    public async Task<PartnerProfile?> GetProfileAsync(long telegramUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting partner profile for TelegramUserId: {Id}", telegramUserId);

        var cacheKey = $"partner_profile_{telegramUserId}";

        // Проверяем кэш — профиль запрашивается при каждом открытии кабинета
        if (cache.TryGetValue(cacheKey, out PartnerProfile? cached))
        {
            logger.LogDebug("Partner profile served from cache for TelegramUserId: {Id}", telegramUserId);
            return cached;
        }

        var account = await accountsStorage.GetByTelegramUserIdAsync(telegramUserId, ct);
        if (account is null)
        {
            logger.LogWarning("Account not found for TelegramUserId: {Id}", telegramUserId);
            return null;
        }

        var user = await usersStorage.GetByIdAsync(telegramUserId, ct);
        var level = CalculateLevel(account.InvitedPurchasesCount);

        var profile = new PartnerProfile
        {
            AccountId = account.Id,
            TelegramUserId = telegramUserId,
            FirstName = user?.FirstName ?? string.Empty,
            LastName = user?.LastName ?? string.Empty,
            Username = user?.Username,
            BonusBalance = account.BonusBalance,
            TotalBonusEarned = account.TotalBonusEarned,
            InvitedCount = account.InvitedPurchasesCount,
            InvitedPurchasesCount = account.InvitedPurchasesCount,
            Level = level
        };

        cache.Set(cacheKey, profile, CacheTtl);
        return profile;
    }

    /// <summary>
    /// Инвалидирует кэш профиля при изменении баланса.
    /// Вызывается из BonusService после любой операции с бонусами.
    /// </summary>
    public void InvalidateProfileCache(long telegramUserId)
    {
        cache.Remove($"partner_profile_{telegramUserId}");
        logger.LogDebug("Profile cache invalidated for TelegramUserId: {Id}", telegramUserId);
    }

    public async Task<string?> GetReferrerNameByTelegramIdAsync(long telegramUserId, CancellationToken ct = default)
    {
        logger.LogDebug("Getting referrer name for TelegramUserId: {Id}", telegramUserId);

        var account = await accountsStorage.GetByTelegramUserIdAsync(telegramUserId, ct);
        if (account?.ReferrerId is null) return null;

        var referrerAccount = await accountsStorage.GetByIdAsync(account.ReferrerId.Value, ct);
        if (referrerAccount is null) return null;

        var referrerUser = await usersStorage.GetByIdAsync(referrerAccount.TelegramUserId, ct);
        if (referrerUser is null) return null;

        var name = $"{referrerUser.FirstName} {referrerUser.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? referrerUser.Username : name;
    }

    public async Task<IEnumerable<PartnerProfile>> GetAllPartnersAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Getting all partners");

        // Получаем всех пользователей с IsPartner = true
        // Storage не поддерживает фильтрацию по IsPartner — делаем через DB напрямую
        // Это нормально для простого pet-проекта, в production выделили бы метод в storage
        var partnerUsers = await usersStorage.GetAllPartnersAsync(ct);

        var profiles = new List<PartnerProfile>();
        foreach (var user in partnerUsers)
        {
            var profile = await GetProfileAsync(user.Id, ct);
            if (profile is not null)
                profiles.Add(profile);
        }

        return profiles;
    }

    private static UserLevel CalculateLevel(int invitedPurchasesCount) => invitedPurchasesCount switch
    {
        <= 2 => UserLevel.Intern,
        <= 5 => UserLevel.Junior,
        <= 10 => UserLevel.Middle,
        <= 20 => UserLevel.Senior,
        _ => UserLevel.Ambassador
    };
}
