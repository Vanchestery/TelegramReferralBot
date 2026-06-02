namespace ReferralBot.Core.Models;

/// <summary>
/// Уровень партнёра в реферальной программе.
/// Определяется динамически по количеству рефералов, совершивших покупку.
/// </summary>
public enum UserLevel
{
    /// <summary>0–2 рефералов. Бонус: 15%.</summary>
    Intern = 0,

    /// <summary>3–5 рефералов. Бонус: 20%.</summary>
    Junior = 1,

    /// <summary>6–10 рефералов. Бонус: 25%.</summary>
    Middle = 2,

    /// <summary>11–20 рефералов. Бонус: 27%.</summary>
    Senior = 3,

    /// <summary>21+ рефералов. Бонус: 30%.</summary>
    Ambassador = 4
}
