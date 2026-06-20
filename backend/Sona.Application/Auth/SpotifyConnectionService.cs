using Sona.Application.Abstractions;

namespace Sona.Application.Auth;

public class SpotifyConnectionService(ISpotifyConnectionGateway connectionGateway)
{
    public string CreateAuthorizationUrl()
    {
        return connectionGateway.CreateAuthorizationUrl();
    }

    public async Task<SpotifyConnectionResult> CompleteAuthorizationAsync(
        string code,
        string state,
        CancellationToken cancellationToken = default)
    {
        return await connectionGateway.CompleteAuthorizationAsync(code, state, cancellationToken);
    }

    public bool IsConnected(string? sessionId)
    {
        return connectionGateway.IsConnected(sessionId);
    }

    public void Disconnect(string? sessionId)
    {
        connectionGateway.Disconnect(sessionId);
    }
}
