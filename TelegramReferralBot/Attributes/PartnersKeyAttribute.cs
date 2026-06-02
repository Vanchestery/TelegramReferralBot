using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ReferralBot.Attributes;

/// <summary>
/// Фильтр авторизации для внешних интеграций (платёжная система, админ-панель).
/// Проверяет наличие секретного ключа в заголовке X-Partners-Key.
///
/// Использование: [PartnersKey] на контроллере или методе.
/// </summary>
public class PartnersKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string HeaderName = "X-Partners-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedKey = config["PARTNERS_API_KEY"];

        if (string.IsNullOrEmpty(expectedKey))
        {
            // Ключ не настроен — пропускаем (для локальной разработки)
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey)
            || providedKey != expectedKey)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid or missing API key" });
            return;
        }

        await next();
    }
}
