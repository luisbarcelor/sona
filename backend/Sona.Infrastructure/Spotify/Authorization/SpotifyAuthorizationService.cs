using System.Net;
using Microsoft.Extensions.Options;
using Sona.Infrastructure.Spotify.Configuration;
using Sona.Infrastructure.Spotify.Models;

namespace Sona.Infrastructure.Spotify.Authorization;

public class SpotifyAuthorizationService(
    SpotifyAuthClient authClient,
    DevelopmentSpotifyTokenStore tokenStore,
    IOptions<SpotifyOptions> options)
{
    public string CreateAuthorizationUrl()
    {
        var (clientId, _) = GetCredentials();
        var state = tokenStore.CreateState();

        var parameters = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = clientId,
            ["scope"] = options.Value.Scope,
            ["redirect_uri"] = options.Value.RedirectUri,
            ["state"] = state
        };

        var query = string.Join("&", parameters.Select(parameter =>
            $"{WebUtility.UrlEncode(parameter.Key)}={WebUtility.UrlEncode(parameter.Value)}"));

        return $"{options.Value.AccountsBaseUrl.TrimEnd('/')}/authorize?{query}";
    }

    public async Task<SpotifyAuthorizationResult> CompleteAuthorizationAsync(
        string code,
        string state,
        CancellationToken cancellationToken = default)
    {
        if (!tokenStore.ValidateState(state))
        {
            throw new ArgumentException("Spotify callback state is invalid.", nameof(state));
        }

        var (clientId, clientSecret) = GetCredentials();
        var token = await authClient.ExchangeCodeForTokenAsync(
            clientId,
            clientSecret,
            code,
            options.Value.RedirectUri,
            cancellationToken);

        var sessionId = tokenStore.SaveAuthorization(token);
        return new SpotifyAuthorizationResult(token, sessionId);
    }

    public async Task<string?> GetAccessTokenAsync(
        string? sessionId,
        CancellationToken cancellationToken = default)
    {
        var storedToken = tokenStore.Get(sessionId);

        if (storedToken is null)
        {
            return null;
        }

        if (storedToken.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
        {
            return storedToken.AccessToken;
        }

        if (string.IsNullOrWhiteSpace(storedToken.RefreshToken))
        {
            throw new SpotifyApiException(
                HttpStatusCode.Unauthorized,
                "Spotify session expired. Connect Spotify again.");
        }

        var (clientId, clientSecret) = GetCredentials();
        var refreshedToken = await authClient.RefreshAccessTokenAsync(
            clientId,
            clientSecret,
            storedToken.RefreshToken,
            cancellationToken);

        tokenStore.SaveRefresh(refreshedToken, storedToken.SessionId);
        return refreshedToken.AccessToken;
    }

    public bool IsConnected(string? sessionId)
    {
        return tokenStore.Get(sessionId) is not null;
    }

    public void Disconnect(string? sessionId)
    {
        tokenStore.Clear(sessionId);
    }

    private (string ClientId, string ClientSecret) GetCredentials()
    {
        var clientId = options.Value.ClientId;
        var clientSecret = options.Value.ClientSecret;

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException(
                "Configure Spotify:ClientId and Spotify:ClientSecret using user secrets before connecting Spotify.");
        }

        return (clientId, clientSecret);
    }
}

public record SpotifyAuthorizationResult(SpotifyTokenResponse Token, string SessionId);
