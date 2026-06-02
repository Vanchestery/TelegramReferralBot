namespace ReferralBot.Db.Entities;

/// <summary>
/// Журнал операций с бонусным балансом. Записи только добавляются — иммутабельный лог.
/// Хранит баланс до и после операции для полной истории и аудита.
/// </summary>
public class BonusTransactionEntity
{
    /// <summary>Уникальный идентификатор транзакции.</summary>
    public Guid Id { get; set; }

    /// <summary>Аккаунт, чей баланс изменился.</summary>
    public Guid AccountId { get; set; }

    /// <summary>Сумма изменения: положительная = начисление, отрицательная = списание.</summary>
    public int Amount { get; set; }

    /// <summary>
    /// ID транзакции из внешней платёжной системы (Stepik).
    /// Используется для idempotency-проверки: не обрабатывать одну транзакцию дважды.
    /// </summary>
    public int PaymentTransactionId { get; set; }

    /// <summary>Время исходной платёжной операции.</summary>
    public DateTime PaymentTime { get; set; }

    /// <summary>Тип операции — определяет бизнес-логику начисления/списания.</summary>
    public BonusOperationType OperationType { get; set; }

    /// <summary>Дата записи транзакции в систему.</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Баланс аккаунта до операции — для аудита и истории.</summary>
    public int BalanceBefore { get; set; }

    /// <summary>Баланс аккаунта после операции.</summary>
    public int BalanceAfter { get; set; }

    /// <summary>ID курса при операциях типа CoursePurchase. 0 если не применимо.</summary>
    public int PurchasedCourseId { get; set; } = 0;
}
