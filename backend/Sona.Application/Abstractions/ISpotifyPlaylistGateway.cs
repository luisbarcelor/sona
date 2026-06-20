using Sona.Application.DTOs;

namespace Sona.Application.Abstractions;

public interface ISpotifyPlaylistGateway
{
    Task<PagedResponseDto<PlaylistDto>> GetCurrentUserPlaylistsAsync(
        string accessToken,
        int limit,
        int offset,
        CancellationToken cancellationToken = default);

    Task<PagedResponseDto<PlaylistItemDto>> GetPlaylistItemsAsync(
        string accessToken,
        string playlistId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default);
}
