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
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
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
    /// Получает список курсов преподавателя.
    /// Polly автоматически повторяет запрос при 5xx ошибках (до 3 раз с exponential backoff).
    /// </summary>
    public async Task<IEnumerable<StepikCourse>> GetTeacherCoursesAsync(
        int teacherId, string accessToken, CancellationToken ct = default)
    {
        logger.LogDebug("Fetching courses for teacher {TeacherId}", teacherId);

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await httpClient.GetAsync(
                $"https://stepik.org/api/courses?teacher={teacherId}", ct);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var root = JsonSerializer.Deserialize<CoursesResponse>(json, JsonOptions);

            logger.LogInformation("Fetched {Count} courses for teacher {TeacherId}",
                root?.Courses?.Count ?? 0, teacherId);

            return root?.Courses ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch courses for teacher {TeacherId}", teacherId);
            return [];
        }
    }

    // DTO для десериализации ответов Stepik API
    private record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
    private record CoursesResponse([property: JsonPropertyName("courses")] List<StepikCourse> Courses);
}
