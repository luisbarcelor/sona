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

    public async Task<PagedResponseDto<PlaylistItemDto>> GetPlaylistItemsAsync(
        string accessToken,
        string playlistId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await spotifyClient.GetPlaylistItemsAsync(
                accessToken,
                playlistId,
                limit,
                offset,
                cancellationToken);

            return new PagedResponseDto<PlaylistItemDto>
            {
                Href = response.Href,
                Limit = response.Limit,
                Next = response.Next,
                Offset = response.Offset,
                Previous = response.Previous,
                Total = response.Total,
                Items = response.Items.Select(MapPlaylistItem).ToList()
            };
        }
        catch (SpotifyApiException exception)
        {
            throw new SpotifyProviderException(exception.StatusCode, exception.Message);
        }
    }

    public async Task<PlaylistDto> GetPlaylistAsync(
        string accessToken,
        string playlistId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var playlist = await spotifyClient.GetPlaylistAsync(
                accessToken,
                playlistId,
                cancellationToken);

            return MapPlaylist(playlist);
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
            Images = MapImages(playlist.Images),
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

    private static List<ImageDto> MapImages(IEnumerable<SpotifyImage>? images)
    {
        return images?.Select(MapImage).ToList() ?? [];
    }

    private static SpotifyExternalUrlsDto MapExternalUrls(SpotifyExternalUrls externalUrls)
    {
        return new SpotifyExternalUrlsDto
        {
            Spotify = externalUrls.Spotify
        };
    }

    private static PlaylistItemDto MapPlaylistItem(SpotifyPlaylistItem playlistItem)
    {
        var item = playlistItem.Item ?? playlistItem.DeprecatedTrack;

        if (item is null)
        {
            return new PlaylistItemDto
            {
                AddedAt = playlistItem.AddedAt,
                IsLocal = playlistItem.IsLocal,
                Item = null,
                UnsupportedReason = "Spotify did not return details for this playlist item."
            };
        }

        if (!string.Equals(item.Type, "track", StringComparison.OrdinalIgnoreCase))
        {
            return new PlaylistItemDto
            {
                AddedAt = playlistItem.AddedAt,
                IsLocal = playlistItem.IsLocal,
                Item = null,
                UnsupportedReason = "Only Spotify track items are supported in this editor."
            };
        }

        return new PlaylistItemDto
        {
            AddedAt = playlistItem.AddedAt,
            IsLocal = playlistItem.IsLocal || item.IsLocal == true,
            Item = MapTrack(item),
            UnsupportedReason = null
        };
    }

    private static TrackDto MapTrack(SpotifyPlaylistItemDetails item)
    {
        return new TrackDto
        {
            Id = item.Id,
            Uri = item.Uri,
            Href = item.Href,
            Name = string.IsNullOrWhiteSpace(item.Name) ? "Unavailable track" : item.Name,
            Type = item.Type ?? "track",
            DurationMs = item.DurationMs,
            Explicit = item.Explicit,
            IsPlayable = item.IsPlayable,
            ExternalUrls = MapExternalUrls(item.ExternalUrls),
            Album = item.Album is null ? null : MapAlbum(item.Album),
            Artists = item.Artists.Select(MapArtist).ToList()
        };
    }

    private static PlaylistTrackAlbumDto MapAlbum(SpotifyPlaylistAlbum album)
    {
        return new PlaylistTrackAlbumDto
        {
            Id = album.Id,
            Uri = album.Uri,
            Href = album.Href,
            Name = string.IsNullOrWhiteSpace(album.Name) ? "Unknown album" : album.Name,
            Images = MapImages(album.Images),
            ReleaseDate = album.ReleaseDate,
            ExternalUrls = MapExternalUrls(album.ExternalUrls)
        };
    }

    private static PlaylistTrackArtistDto MapArtist(SpotifyPlaylistArtist artist)
    {
        return new PlaylistTrackArtistDto
        {
            Id = artist.Id,
            Uri = artist.Uri,
            Href = artist.Href,
            Name = string.IsNullOrWhiteSpace(artist.Name) ? "Unknown artist" : artist.Name,
            ExternalUrls = MapExternalUrls(artist.ExternalUrls)
        };
    }

    private static SpotifyExternalUrlsDto? MapExternalUrls(SpotifyPlaylistItemExternalUrls? externalUrls)
    {
        if (string.IsNullOrWhiteSpace(externalUrls?.Spotify))
        {
            return null;
        }

        return new SpotifyExternalUrlsDto
        {
            Spotify = externalUrls.Spotify
        };
    }
}
