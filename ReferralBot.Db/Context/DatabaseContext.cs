using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using ReferralBot.Db.Entities;
using ReferralBot.Db.Helpers;

namespace ReferralBot.Db.Context;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<TelegramBotUserEntity> TelegramBotUsers { get; set; }
    public DbSet<TelegramBotUserStateEntity> TelegramUserStates { get; set; }
    public DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<ReferralLinkEntity> ReferralLinks { get; set; }
    public DbSet<PromoCodeEntity> PromoCodes { get; set; }
    public DbSet<BonusTransactionEntity> BonusTransactions { get; set; }
    public DbSet<WelcomeVideoEntity> WelcomeVideos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTelegramBotUser(modelBuilder);
        ConfigureAccount(modelBuilder);
        ConfigureTelegramBotUserState(modelBuilder);
        ConfigureReferralLink(modelBuilder);
        ConfigureBonusTransaction(modelBuilder);
        ConfigurePromoCode(modelBuilder);
        ConfigureWelcomeVideo(modelBuilder);
    }

    private static void ConfigureTelegramBotUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelegramBotUserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Telegram ID задаётся явно — БД его не генерирует
            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.Username)
                .HasMaxLength(32);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.IsPartner)
                .HasDefaultValue(false);
        });
    }

    private static void ConfigureAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.BonusBalance)
                .HasColumnType("integer");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAddOrUpdate();

            // UserDbStatus хранится как строка — читаемо в БД, не зависит от порядка enum-значений
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<UserDbStatus>(v));
        });
    }

    private static void ConfigureTelegramBotUserState(ModelBuilder modelBuilder)
    {
        // Настройки сериализации для jsonb: без переименования полей, компактно
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = false
        };

        modelBuilder.Entity<TelegramBotUserStateEntity>(entity =>
        {
            entity.HasKey(e => e.TelegramUserId);

            entity.HasIndex(e => e.TelegramUserId)
                .IsUnique();

            // PageNames хранится как jsonb. ListOfStringsComparer нужен чтобы EF Core
            // не генерировал лишние UPDATE когда содержимое списка не изменилось.
            entity.Property(e => e.PageNames)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>(),
                    new ListOfStringsComparer());

            entity.Property(e => e.CurrentMessageId)
                .HasDefaultValue(0);

            entity.Property(e => e.IsMediaContent)
                .IsRequired()
                .HasDefaultValue(false);
        });
    }

    private static void ConfigureReferralLink(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReferralLinkEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.AccountId)
                .IsRequired();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            // Один аккаунт — одна реферальная ссылка (защита на уровне БД)
            entity.HasIndex(e => e.AccountId)
                .IsUnique();
        });
    }

    private static void ConfigureBonusTransaction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BonusTransactionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Индекс для idempotency-проверки: не обрабатывать одну платёжную транзакцию дважды
            entity.HasIndex(e => e.PaymentTransactionId);
        });
    }

    private static void ConfigurePromoCode(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PromoCodeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Один аккаунт — один промокод на один курс
            entity.HasIndex(e => new { e.AccountId, e.CourseId })
                .IsUnique();
        });
    }

    private static void ConfigureWelcomeVideo(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WelcomeVideoEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
        });
    }
}
