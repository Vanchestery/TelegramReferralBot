using ReferralBot.Models;

namespace ReferralBot.Services;

/// <summary>
/// Каталог курсов для UI бота. Инкапсулирует поход в Stepik (список/детали/обложка)
/// и кэширование — страницы знают только про CourseIdTitlePair / CourseDetails.
/// </summary>
public interface ICourseService
{
    Task<List<CourseIdTitlePair>> GetCoursesIdTitleAsync(CancellationToken ct = default);
    Task<CourseDetails?> GetCourseInfoAsync(int courseId, CancellationToken ct = default);
    Task<byte[]?> GetCourseLogoAsync(int courseId, CancellationToken ct = default);
}
