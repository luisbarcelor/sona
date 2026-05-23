using System.Net;
using Sona.Infrastructure.Spotify.Configuration;
using Sona.Infrastructure.Spotify.Models;

namespace Sona.Infrastructure.Spotify.Authorization;

public class SpotifyAuthorizationService(
    SpotifyAuthClient authClient,
    DevelopmentSpotifyTokenStore tokenStore,
    SpotifyOptions options)
{
    public string CreateAuthorizationUrl()
    {
        var (clientId, _) = GetCredentials();
        var state = tokenStore.CreateState();

        var parameters = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = clientId,
            ["scope"] = options.Scope,
            ["redirect_uri"] = options.RedirectUri,
            ["state"] = state
        };

        var query = string.Join("&", parameters.Select(parameter =>
            $"{WebUtility.UrlEncode(parameter.Key)}={WebUtility.UrlEncode(parameter.Value)}"));

        return $"{options.AccountsBaseUrl.TrimEnd('/')}/authorize?{query}";
    }

    public async Task<SpotifyTokenResponse> CompleteAuthorizationAsync(
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
            options.RedirectUri,
            cancellationToken);

        tokenStore.Save(token);
        return token;
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var storedToken = tokenStore.Get();

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

        tokenStore.Save(refreshedToken);
        return refreshedToken.AccessToken;
    }

    private (string ClientId, string ClientSecret) GetCredentials()
    {
        var clientId = options.ClientId;
        var clientSecret = options.ClientSecret;

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException(
                "Configure Spotify:ClientId and Spotify:ClientSecret using user secrets before connecting Spotify.");
        }

        return (clientId, clientSecret);
    }
}
