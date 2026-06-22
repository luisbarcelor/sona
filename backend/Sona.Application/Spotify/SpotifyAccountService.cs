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

    public async Task<PlaylistEditorDto> GetPlaylistEditorAsync(
        string? sessionId,
        string playlistId,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GetRequiredAccessTokenAsync(sessionId, cancellationToken);
        const int pageSize = 50;

        try
        {
            var playlist = await playlistGateway.GetPlaylistAsync(
                accessToken,
                playlistId,
                cancellationToken);
            var firstPage = await playlistGateway.GetPlaylistItemsAsync(
                accessToken,
                playlistId,
                pageSize,
                0,
                cancellationToken);
            var items = new List<PlaylistItemDto>(firstPage.Items);

            for (var offset = pageSize; offset < firstPage.Total; offset += pageSize)
            {
                var page = await playlistGateway.GetPlaylistItemsAsync(
                    accessToken,
                    playlistId,
                    pageSize,
                    offset,
                    cancellationToken);

                items.AddRange(page.Items);
            }

            if (items.Count != firstPage.Total)
            {
                throw new InvalidOperationException("Spotify returned an incomplete playlist item response.");
            }

            return new PlaylistEditorDto
            {
                PlaylistId = playlistId,
                SnapshotId = playlist.SnapshotId,
                Total = firstPage.Total,
                Items = items
            };
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
