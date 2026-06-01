using System.Diagnostics;

namespace ReferralBot.Middleware;

/// <summary>
/// Middleware логирования входящих HTTP-запросов с временем обработки.
/// Подключается в Program.cs через app.UseMiddleware&lt;RequestLoggingMiddleware&gt;().
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        await next(context);

        sw.Stop();

        logger.LogInformation(
            "{Method} {Path} → {StatusCode} ({ElapsedMs}ms)",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
}
