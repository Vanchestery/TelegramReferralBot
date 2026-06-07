using ReferralBot.Core.Models;

namespace ReferralBot.Services;

/// <summary>
/// Контракт клиента Stepik API.
///
/// Вынесен из StepikApiClient отдельным интерфейсом, чтобы зависящие сервисы
/// (CourseService) можно было покрыть unit-тестами с моком — без реальных
/// HTTP-запросов к Stepik. Совпадает со стилем «interface-per-service» в проекте.
/// </summary>
public interface IStepikApiClient
{
    /// <summary>
    /// OAuth2-токен через client_credentials.
    /// Возвращает null, если креды не заданы или запрос не удался.
    /// </summary>
    Task<string?> GetAccessTokenAsync(CancellationToken ct = default);

    /// <summary>
    /// Курсы преподавателя (все страницы). accessToken опционален — публичные курсы
    /// доступны без авторизации. Возвращает пустую коллекцию при ошибке.
    /// </summary>
    Task<IEnumerable<StepikCourse>> GetTeacherCoursesAsync(
        int teacherId, string? accessToken = null, CancellationToken ct = default);

    /// <summary>Один курс по id (детали). null при ошибке. Токен опционален.</summary>
    Task<StepikCourse?> GetCourseByIdAsync(
        int courseId, string? accessToken = null, CancellationToken ct = default);
}
