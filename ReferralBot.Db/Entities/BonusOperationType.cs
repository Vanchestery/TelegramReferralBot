namespace ReferralBot.Db.Entities;

/// <summary>
/// Тип операции с бонусным балансом.
/// Хранится как int — для транзакций важна компактность и скорость фильтрации.
/// </summary>
public enum BonusOperationType
{
    /// <summary>Начисление бонусов реферерру за покупку реферала.</summary>
    Purchase = 1,

    /// <summary>Списание бонусов при возврате покупки реферала.</summary>
    Refund = 2,

    /// <summary>Списание бонусов при покупке курса самим партнёром.</summary>
    CoursePurchase = 3,

    /// <summary>Ручная корректировка администратором.</summary>
    ManualOperation = 4,

    /// <summary>Списание по внешнему запросу.</summary>
    Debit = 5,

    /// <summary>Начисление по внешнему запросу.</summary>
    Credit = 6
}
