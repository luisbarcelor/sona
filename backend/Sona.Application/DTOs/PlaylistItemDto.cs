using System.Text.Json.Serialization;

namespace Sona.Application.DTOs;

public class PlaylistItemDto
{
    [JsonPropertyName("added_at")]
    public DateTimeOffset? AddedAt { get; set; }

    [JsonPropertyName("is_local")]
    public required bool IsLocal { get; set; }

    public TrackDto? Item { get; set; }

    [JsonPropertyName("unsupported_reason")]
    public string? UnsupportedReason { get; set; }
}

public class TrackDto
{
    public string? Id { get; set; }
    public string? Uri { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Href { get; set; }

    [JsonPropertyName("duration_ms")]
    public int? DurationMs { get; set; }

    public bool? Explicit { get; set; }

    [JsonPropertyName("is_playable")]
    public bool? IsPlayable { get; set; }

    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrlsDto? ExternalUrls { get; set; }

    public PlaylistTrackAlbumDto? Album { get; set; }
    public required List<PlaylistTrackArtistDto> Artists { get; set; }
}

public class PlaylistTrackAlbumDto
{
    public string? Id { get; set; }
    public string? Uri { get; set; }
    public string? Href { get; set; }
    public required string Name { get; set; }
    public required List<ImageDto> Images { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrlsDto? ExternalUrls { get; set; }
}

public class PlaylistTrackArtistDto
{
    public string? Id { get; set; }
    public string? Uri { get; set; }
    public string? Href { get; set; }
    public required string Name { get; set; }

    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrlsDto? ExternalUrls { get; set; }
}
