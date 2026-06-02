namespace ReferralBot.Pages;

/// <summary>
/// Фабрика страниц по полному имени типа.
/// Используется в PageStackConverter при десериализации стека из БД.
///
/// Резолвит тип через Type.GetType(fullName) и создаёт через DI.
/// </summary>
public class PagesFactory(IServiceProvider services, ILogger<PagesFactory> logger)
{
    public IPage GetPage(string fullTypeName)
    {
        logger.LogDebug("Resolving page type: {TypeName}", fullTypeName);

        var type = Type.GetType(fullTypeName)
            ?? throw new InvalidOperationException($"Page type not found: {fullTypeName}");

        return (IPage)services.GetRequiredService(type);
    }
}
