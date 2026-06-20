using Sona.Application.Abstractions;
using Sona.Application.DTOs;
using Sona.Infrastructure.Spotify.Models;

namespace Sona.Infrastructure.Spotify.Api;

public class SpotifyPlaylistGateway(SpotifyClient spotifyClient) : ISpotifyPlaylistGateway
{
    public async Task<PagedResponseDto<PlaylistDto>> GetCurrentUserPlaylistsAsync(
        string accessToken,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await spotifyClient.GetCurrentUserPlaylistsAsync(
                accessToken,
                limit,
                offset,
                cancellationToken);

            return new PagedResponseDto<PlaylistDto>
            {
                Href = response.Href,
                Limit = response.Limit,
                Next = response.Next,
                Offset = response.Offset,
                Previous = response.Previous,
                Total = response.Total,
                Items = response.Items.Select(MapPlaylist).ToList()
            };
        }
        catch (SpotifyApiException exception)
        {
            throw new SpotifyProviderException(exception.StatusCode, exception.Message);
        }
    }

    private static PlaylistDto MapPlaylist(SpotifyPlaylist playlist)
    {
        return new PlaylistDto
        {
            Collaborative = playlist.Collaborative,
            Description = playlist.Description,
            ExternalUrls = MapExternalUrls(playlist.ExternalUrls),
            Href = playlist.Href,
            Id = playlist.Id,
            Images = playlist.Images.Select(MapImage).ToList(),
            Name = playlist.Name,
            Owner = new PlaylistOwnerDto
            {
                ExternalUrls = MapExternalUrls(playlist.Owner.ExternalUrls),
                Href = playlist.Owner.Href,
                Id = playlist.Owner.Id,
                Type = playlist.Owner.Type,
                Uri = playlist.Owner.Uri,
                DisplayName = playlist.Owner.DisplayName
            },
            Public = playlist.Public,
            SnapshotId = playlist.SnapshotId,
            Items = new PlaylistTracksReferenceDto
            {
                Href = playlist.Items.Href,
                Total = playlist.Items.Total
            },
            Type = playlist.Type,
            Uri = playlist.Uri
        };
    }

    private static ImageDto MapImage(SpotifyImage image)
    {
        return new ImageDto
        {
            Url = image.Url,
            Height = image.Height,
            Width = image.Width
        };
    }

    private static SpotifyExternalUrlsDto MapExternalUrls(SpotifyExternalUrls externalUrls)
    {
        return new SpotifyExternalUrlsDto
        {
            Spotify = externalUrls.Spotify
        };
    }
}
