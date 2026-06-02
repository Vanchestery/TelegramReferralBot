namespace ReferralBot.Core.Models;

/// <summary>
/// Статус пользователя на уровне доменной модели.
/// Зеркалит UserDbStatus из слоя данных — разделение намеренное:
/// слой Core не должен зависеть от деталей хранения.
/// </summary>
public enum UserStatus
{
    Active,
    Banned,
    Deleted
}
