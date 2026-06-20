using System.Net;
using Sona.Application.Abstractions;
using Sona.Application.Auth;
using Sona.Application.DTOs;

namespace Sona.Application.Spotify;

public class SpotifyAccountService(
    ISpotifyConnectionGateway connectionGateway,
    ISpotifyProfileGateway profileGateway,
    ISpotifyPlaylistGateway playlistGateway)
{
    public async Task<CurrentUserProfileDto> GetCurrentUserProfileAsync(
        string? sessionId,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GetRequiredAccessTokenAsync(sessionId, cancellationToken);

        try
        {
            return await profileGateway.GetCurrentUserProfileAsync(accessToken, cancellationToken);
        }
        catch (SpotifyProviderException exception)
            when (exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            connectionGateway.Disconnect(sessionId);
            throw;
        }
    }

    public async Task<PagedResponseDto<PlaylistDto>> GetCurrentUserPlaylistsAsync(
        string? sessionId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GetRequiredAccessTokenAsync(sessionId, cancellationToken);

        try
        {
            return await playlistGateway.GetCurrentUserPlaylistsAsync(
                accessToken,
                limit,
                offset,
                cancellationToken);
        }
        catch (SpotifyProviderException exception)
            when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            connectionGateway.Disconnect(sessionId);
            throw;
        }
    }

    public async Task<PagedResponseDto<PlaylistItemDto>> GetPlaylistItemsAsync(
        string? sessionId,
        string playlistId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GetRequiredAccessTokenAsync(sessionId, cancellationToken);

        try
        {
            return await playlistGateway.GetPlaylistItemsAsync(
                accessToken,
                playlistId,
                limit,
                offset,
                cancellationToken);
        }
        catch (SpotifyProviderException exception)
            when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            connectionGateway.Disconnect(sessionId);
            throw;
        }
    }

    private async Task<string> GetRequiredAccessTokenAsync(
        string? sessionId,
        CancellationToken cancellationToken)
    {
        var accessToken = await connectionGateway.GetAccessTokenAsync(sessionId, cancellationToken);

        return accessToken ?? throw new SpotifyConnectionRequiredException(
            "Spotify is not connected. Open GET /spotify/connect first.");
    }
}
