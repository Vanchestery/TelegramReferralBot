namespace ReferralBot.Services;

/// <summary>
/// Создаёт экземпляры страниц через DI с правильным временем жизни.
///
/// Зачем IServiceScopeFactory а не IServiceProvider напрямую?
/// PageCreator сам зарегистрирован как Scoped, но страницы тоже Scoped.
/// Создание через тот же scope корректно — страница получит те же зависимости.
/// </summary>
public class PageCreator(IServiceScopeFactory serviceScopeFactory)
{
    public T CreatePage<T>() where T : ReferralBot.Pages.IPage
    {
        // Scope создаётся здесь и живёт вместе со страницей.
        // Не используем using — scope должен жить пока живёт страница.
        var scope = serviceScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}
