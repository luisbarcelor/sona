using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Sona.Infrastructure.Spotify.Models;

namespace Sona.Infrastructure.Spotify.Authorization;

public class SpotifyAuthClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SpotifyTokenResponse> ExchangeCodeForTokenAsync(
        string clientId,
        string clientSecret,
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateTokenRequest(clientId, clientSecret, new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri
        });

        return await SendTokenRequestAsync(request, cancellationToken);
    }

    public async Task<SpotifyTokenResponse> RefreshAccessTokenAsync(
        string clientId,
        string clientSecret,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateTokenRequest(clientId, clientSecret, new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        });

        return await SendTokenRequestAsync(request, cancellationToken);
    }

    private static HttpRequestMessage CreateTokenRequest(
        string clientId,
        string clientSecret,
        Dictionary<string, string> form)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/token")
        {
            Content = new FormUrlEncodedContent(form)
        };

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        return request;
    }

    private async Task<SpotifyTokenResponse> SendTokenRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>(JsonOptions, cancellationToken);

            return token ?? throw new SpotifyApiException(
                response.StatusCode,
                "Spotify returned an empty token response.");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = content;

        try
        {
            var error = JsonSerializer.Deserialize<SpotifyTokenErrorResponse>(content, JsonOptions);
            message = error?.ErrorDescription ?? error?.Error ?? content;
        }
        catch (JsonException)
        {
        }

        throw new SpotifyApiException(response.StatusCode, message);
    }
}
