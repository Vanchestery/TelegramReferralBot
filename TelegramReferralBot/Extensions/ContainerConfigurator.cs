using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Polly;

using ReferralBot.Core.Interfaces;
using ReferralBot.Core.Mappings;
using ReferralBot.Core.Services;
using ReferralBot.Db.Context;
using ReferralBot.Db.Interfaces;
using ReferralBot.Db.Storage;
using ReferralBot.Pages;
using ReferralBot.Services;
using ReferralBot.Services.Bot;

using Telegram.Bot;

namespace ReferralBot.Extensions;

public static class ContainerConfigurator
{
    public static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        ConfigureBot(services);
        ConfigureTelegramBotClient(services);
        ConfigureDbContext(services);
        RegisterStorages(services);
        RegisterCoreServices(services);
        RegisterBotServices(services);
        RegisterPages(services);
        ConfigureAutoMapper(services);

        services.AddMemoryCache();

        ConfigureStepikApiClient(services);

        // Курсы: ICourseService поверх Stepik (список/детали/обложка + кэш) и
        // IPromoCodeService — hex промокода партнёра для ссылки оплаты со скидкой.
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IPromoCodeService, PromoCodeService>();

        // BackgroundService — ежедневная рассылка статистики партнёрам
        services.AddHostedService<DailyStatsNotificationService>();

        services.AddSingleton<IHostedService>(sp =>
            new WebHookConfigurator(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<ILogger<WebHookConfigurator>>()));
    }

    // ──────────────────────────────────────────────────────────────────────────

    private static void ConfigureBot(IServiceCollection services)
    {
        // Читаем из IConfiguration — подхватывает и env-переменные, и appsettings, и user-secrets
        services.AddOptions<BotConfiguration>()
            .Configure<IConfiguration>((cfg, config) =>
            {
                cfg.Token = config["REF_BOT_KEY"] ?? string.Empty;
                cfg.WebhookUrl = config["REF_BOT_WEBHOOK_URL"] ?? string.Empty;
            })
            .Validate(cfg =>
                !string.IsNullOrEmpty(cfg.Token) &&
                !string.IsNullOrEmpty(cfg.WebhookUrl),
                "REF_BOT_KEY and REF_BOT_WEBHOOK_URL must be set")
            .ValidateOnStart();
    }

    private static void ConfigureTelegramBotClient(IServiceCollection services)
    {
        services.AddHttpClient("telegram_bot")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var config = sp.GetRequiredService<IOptions<BotConfiguration>>().Value;
                return new TelegramBotClient(config.Token, httpClient);
            });
    }

    private static void ConfigureDbContext(IServiceCollection services)
    {
        services.AddDbContext<DatabaseContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config["POSTGRES_REFERRALBOT_DB"]
                ?? throw new InvalidOperationException("POSTGRES_REFERRALBOT_DB is not set");

            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        },
        contextLifetime: ServiceLifetime.Scoped,
        optionsLifetime: ServiceLifetime.Singleton);
    }

    private static void RegisterStorages(IServiceCollection services)
    {
        services.AddScoped<ITelegramBotUsersStorage, TelegramBotUsersStorage>();
        services.AddScoped<ITelegramUserStatesStorage, TelegramUserStatesStorage>();
        services.AddScoped<IAccountsStorage, AccountsStorage>();
        services.AddScoped<IReferralLinksStorage, ReferralLinksStorage>();
        services.AddScoped<IBonusTransactionStorage, BonusTransactionStorage>();
        services.AddScoped<IPromoCodesStorage, PromoCodesStorage>();
        services.AddScoped<IWelcomeVideoStorage, WelcomeVideoStorage>();
    }

    private static void RegisterCoreServices(IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITelegramBotUserService, TelegramBotUserService>();
        services.AddScoped<ITelegramBotUserStatesService, TelegramBotUserStatesService>();
        services.AddScoped<IReferralLinkService, ReferralLinkService>();
        services.AddScoped<IBonusService, BonusService>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IWelcomeVideoService, WelcomeVideoService>();
    }

    private static void RegisterBotServices(IServiceCollection services)
    {
        services.AddScoped<IBotService, BotService>();
        services.AddScoped<CommandsProvider>();
        services.AddScoped<PageCreator>();
        services.AddScoped<PageStackConverter>();
        services.AddScoped<TelegramUserContextConverter>();
        services.AddScoped<PagesFactory>();
    }

    /// <summary>
    /// Регистрирует все страницы бота через рефлексию.
    /// Любой класс реализующий IPage и не являющийся абстрактным — попадёт в DI.
    /// Новые страницы добавляются автоматически без изменения этого метода.
    /// </summary>
    private static void RegisterPages(IServiceCollection services)
    {
        var pageTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(IPage).IsAssignableFrom(t));

        foreach (var type in pageTypes)
            services.AddScoped(type);
    }

    /// <summary>
    /// Регистрирует typed HTTP-клиент для Stepik API с retry-политикой Polly.
    ///
    /// Exponential backoff: 1я попытка — сразу, 2я — через 2с, 3я — через 4с.
    /// Срабатывает при HttpRequestException и ответах 5xx (сервер недоступен).
    /// </summary>
    private static void ConfigureStepikApiClient(IServiceCollection services)
    {
        services.AddHttpClient<IStepikApiClient, StepikApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://stepik.org/api/");
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, attempt, _) =>
                {
                    var reason = outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString();
                    Console.WriteLine($"[Stepik] Retry {attempt} after {timespan.TotalSeconds}s. Reason: {reason}");
                }));
    }

    private static void ConfigureAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<AccountProfile>();
            cfg.AddProfile<TelegramBotUserProfile>();
            cfg.AddProfile<TelegramBotUserStateProfile>();
            cfg.AddProfile<ReferralLinkProfile>();
            cfg.AddProfile<BonusTransactionProfile>();
        });
    }

}
