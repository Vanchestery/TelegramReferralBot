using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ReferralBot.Db.Context;

/// <summary>
/// Фабрика для создания DatabaseContext во время выполнения команд dotnet ef.
/// Нужна потому что DatabaseContext живёт в отдельном проекте без точки входа.
///
/// Использование:
///   dotnet ef migrations add InitialCreate --project ReferralBot.Db --startup-project TelegramReferralBot
///   dotnet ef database update            --project ReferralBot.Db --startup-project TelegramReferralBot
/// </summary>
public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_REFERRALBOT_DB")
            ?? "Host=localhost;Port=5434;Database=referralbot;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new DatabaseContext(optionsBuilder.Options);
    }
}
