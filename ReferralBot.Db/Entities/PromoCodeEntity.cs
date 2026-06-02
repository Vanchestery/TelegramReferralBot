using System.ComponentModel.DataAnnotations;

namespace ReferralBot.Db.Entities;

/// <summary>
/// Промокод, привязанный к аккаунту партнёра и конкретному курсу.
/// Один аккаунт может иметь не более одного промокода на каждый курс.
/// </summary>
public class PromoCodeEntity
{
    [Key]
    public int Id { get; set; }

    /// <summary>Аккаунт-владелец промокода.</summary>
    public Guid AccountId { get; set; }

    /// <summary>ID курса на платформе Stepik.</summary>
    public int CourseId { get; set; }

    /// <summary>Публичное название промокода.</summary>
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Промокод активен и доступен для использования.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Описание условий применения.</summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Размер скидки (число или текст, например "10" или "10%").</summary>
    [MaxLength(50)]
    public string Discount { get; set; } = string.Empty;

    /// <summary>Промокод создан и синхронизирован со стороны Stepik.</summary>
    public bool IsStepikSide { get; set; } = false;

    /// <summary>true — скидка в процентах; false — фиксированная сумма.</summary>
    public bool IsPercentDiscount { get; set; } = false;

    /// <summary>
    /// Уникальный хеш для платёжной ссылки:
    /// https://stepik.org/a/{courseId}/pay?promo={Hex}
    /// Генерируется на основе CourseId + AccountId.
    /// </summary>
    [MaxLength(64)]
    public string Hex { get; set; } = string.Empty;

    public DateTime? CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }

    /// <summary>Дата начала действия. null — действует с момента создания.</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Дата окончания действия. null — бессрочный.</summary>
    public DateTime? ExpireDate { get; set; }

    /// <summary>Stepik User ID для персонального промокода. null — доступен всем.</summary>
    public int? UserId { get; set; }

    /// <summary>Максимум использований одним пользователем. null — без ограничений.</summary>
    public int? LimitPerUser { get; set; }
}
