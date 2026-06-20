using Sona.Application.Auth;

namespace Sona.Application.Abstractions;

public interface ISpotifyConnectionGateway
{
    string CreateAuthorizationUrl();

    Task<SpotifyConnectionResult> CompleteAuthorizationAsync(
        string code,
        string state,
        CancellationToken cancellationToken = default);

    Task<string?> GetAccessTokenAsync(
        string? sessionId,
        CancellationToken cancellationToken = default);

    bool IsConnected(string? sessionId);

    void Disconnect(string? sessionId);
}
