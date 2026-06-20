using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Sona.Infrastructure.Spotify.Models;

namespace Sona.Infrastructure.Spotify.Api;

public class SpotifyClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SpotifyCurrentUser> GetCurrentUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("Spotify access token is required.", nameof(accessToken));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "/v1/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await SendWithRetryAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<SpotifyCurrentUser>(
                JsonOptions,
                cancellationToken);

            return user ?? throw new SpotifyApiException(
                HttpStatusCode.OK,
                "Spotify returned an empty user profile response.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    public async Task<SpotifyPagedResponse<SpotifyPlaylist>> GetCurrentUserPlaylistsAsync(
        string accessToken,
        int limit = 20,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("Spotify access token is required.", nameof(accessToken));
        }

        if (limit is < 1 or > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), limit, "Spotify playlist limit must be between 1 and 50.");
        }

        if (offset is < 0 or > 100_000)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Spotify playlist offset must be between 0 and 100000.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/me/playlists?limit={limit}&offset={offset}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await SendWithRetryAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var playlists = await response.Content.ReadFromJsonAsync<SpotifyPagedResponse<SpotifyPlaylist>>(
                JsonOptions,
                cancellationToken);

            return playlists ?? throw new SpotifyApiException(
                HttpStatusCode.OK,
                "Spotify returned an empty playlist response.");
        }

        throw await CreateExceptionAsync(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var response = await httpClient.SendAsync(CloneRequest(request), cancellationToken);

            if (response.StatusCode != HttpStatusCode.TooManyRequests || attempt == maxAttempts)
            {
                return response;
            }

            var delay = GetRetryDelay(response, attempt);
            response.Dispose();

            await Task.Delay(delay, cancellationToken);
        }

        throw new InvalidOperationException("Unexpected Spotify retry state.");
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        if (response.Headers.RetryAfter?.Delta is { } delta)
        {
            return delta;
        }

        if (response.Headers.RetryAfter?.Date is { } date)
        {
            var delay = date - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
        }

        return TimeSpan.FromSeconds(Math.Pow(2, attempt));
    }

    private static async Task<SpotifyApiException> CreateExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var message = await ReadSpotifyErrorMessageAsync(response, cancellationToken);

        return new SpotifyApiException(
            response.StatusCode,
            message ?? $"Spotify API request failed with HTTP {(int)response.StatusCode}.");
    }

    private static async Task<string?> ReadSpotifyErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var error = JsonSerializer.Deserialize<SpotifyErrorResponse>(content, JsonOptions);

            return error?.Error?.Message ?? content;
        }
        catch (JsonException)
        {
            return content;
        }
    }
}
