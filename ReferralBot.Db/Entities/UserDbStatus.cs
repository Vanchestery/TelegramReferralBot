namespace ReferralBot.Db.Entities;

/// <summary>
/// Статус учётной записи пользователя в базе данных.
/// Хранится как строка (HasConversion) — читается без магии чисел.
/// </summary>
public enum UserDbStatus
{
    Active,
    Banned,
    Deleted
}
