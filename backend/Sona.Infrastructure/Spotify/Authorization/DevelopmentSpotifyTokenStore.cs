using System.Collections.Concurrent;
using System.Security.Cryptography;
using Sona.Infrastructure.Spotify.Models;

namespace Sona.Infrastructure.Spotify.Authorization;

public class DevelopmentSpotifyTokenStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _states = new();
    private StoredSpotifyToken? _token;
    public StoredSpotifyToken? Get() => _token;

    public string CreateState()
    {
        var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');

        _states[state] = DateTimeOffset.UtcNow.AddMinutes(10);

        return state;
    }

    public bool ValidateState(string state)
    {
        if (!_states.TryRemove(state, out var expiresAt))
        {
            return false;
        }

        return expiresAt > DateTimeOffset.UtcNow;
    }

    public void Save(SpotifyTokenResponse token)
    {
        var refreshToken = token.RefreshToken ?? _token?.RefreshToken;

        _token = new StoredSpotifyToken(
            token.AccessToken,
            refreshToken,
            DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn),
            token.Scope,
            token.TokenType);
    }
}

public record StoredSpotifyToken(
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset ExpiresAt,
    string? Scope,
    string TokenType);
