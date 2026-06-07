using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

using ReferralBot.Core.Models;

namespace ReferralBot.Services;

/// <summary>
/// Typed HTTP-клиент для Stepik API.
/// Зарегистрирован через IHttpClientFactory с retry-политикой Polly.
///
/// Зачем IHttpClientFactory, а не new HttpClient():
/// new HttpClient() каждый раз создаёт новый сокет — при большом количестве запросов
/// возникает socket exhaustion (исчерпание портов). IHttpClientFactory управляет
/// пулом HttpMessageHandler и переиспользует соединения.
/// </summary>
public class StepikApiClient(HttpClient httpClient, IConfiguration config, ILogger<StepikApiClient> logger)
    : IStepikApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        // Stepik отдаёт snake_case (is_public, display_price, ...). Naming policy
        // маппит их на PascalCase-свойства без атрибутов на каждом поле модели.
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Получает токен OAuth2 через client_credentials.
    /// Stepik использует стандартный OAuth2 flow.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var clientId = config["STEPIK_CLIENT_ID"];
        var clientSecret = config["STEPIK_CLIENT_SECRET"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            logger.LogWarning("Stepik API credentials not configured");
            return null;
        }

        var credentials = Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, "https://stepik.org/oauth2/token/")
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            }),
            Headers = { Authorization = new AuthenticationHeaderValue("Basic", credentials) }
        };

        try
        {
            var response = await httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json, JsonOptions);

            return tokenResponse?.AccessToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get Stepik access token");
            return null;
        }
    }

    /// <summary>
    /// Получает список курсов преподавателя (со всех страниц пагинации).
    ///
    /// Токен необязателен: список публичных курсов Stepik отдаёт и без авторизации.
    /// Если токен передан — добавляем его per-request, не мутируя DefaultRequestHeaders
    /// общего typed-клиента (это безопаснее при конкурентных запросах).
    /// Polly повторяет запрос при 5xx (до 3 раз с exponential backoff).
    /// </summary>
    public async Task<IEnumerable<StepikCourse>> GetTeacherCoursesAsync(
        int teacherId, string? accessToken = null, CancellationToken ct = default)
    {
        const int maxPages = 20; // страховка от бесконечного цикла пагинации
        var all = new List<StepikCourse>();

        for (var page = 1; page <= maxPages; page++)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, $"https://stepik.org/api/courses?teacher={teacherId}&page={page}");

            if (!string.IsNullOrEmpty(accessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await httpClient.SendAsync(request, ct);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(ct);
                var root = JsonSerializer.Deserialize<CoursesResponse>(json, JsonOptions);

                if (root?.Courses is { Count: > 0 })
                    all.AddRange(root.Courses);

                if (root?.Meta?.HasNext != true)
                    break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch courses page {Page} for teacher {TeacherId}", page, teacherId);
                break;
            }
        }

        logger.LogInformation("Fetched {Count} courses for teacher {TeacherId}", all.Count, teacherId);
        return all;
    }

    /// <summary>
    /// Получает один курс по id. Токен необязателен для публичных курсов.
    /// </summary>
    public async Task<StepikCourse?> GetCourseByIdAsync(
        int courseId, string? accessToken = null, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"https://stepik.org/api/courses/{courseId}");

        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var root = JsonSerializer.Deserialize<CoursesResponse>(json, JsonOptions);
            return root?.Courses?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch course {CourseId}", courseId);
            return null;
        }
    }

    // DTO для десериализации ответов Stepik API
    private record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
    private record CoursesMeta([property: JsonPropertyName("has_next")] bool HasNext);
    private record CoursesResponse(
        [property: JsonPropertyName("meta")] CoursesMeta? Meta,
        [property: JsonPropertyName("courses")] List<StepikCourse>? Courses);
}
