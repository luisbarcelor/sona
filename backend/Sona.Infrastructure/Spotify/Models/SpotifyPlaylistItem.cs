using System.Text.Json.Serialization;

namespace Sona.Infrastructure.Spotify.Models;

public class SpotifyPlaylistItem
{
    [JsonPropertyName("added_at")]
    public DateTimeOffset? AddedAt { get; set; }

    [JsonPropertyName("is_local")]
    public bool IsLocal { get; set; }

    public SpotifyPlaylistItemDetails? Item { get; set; }

    [JsonPropertyName("track")]
    public SpotifyPlaylistItemDetails? DeprecatedTrack { get; set; }
}

public class SpotifyPlaylistItemDetails
{
    public string? Id { get; set; }
    public string? Uri { get; set; }
    public string? Href { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }

    [JsonPropertyName("duration_ms")]
    public int? DurationMs { get; set; }

    public bool? Explicit { get; set; }

    [JsonPropertyName("is_playable")]
    public bool? IsPlayable { get; set; }

    [JsonPropertyName("is_local")]
    public bool? IsLocal { get; set; }

    [JsonPropertyName("external_urls")]
    public SpotifyPlaylistItemExternalUrls? ExternalUrls { get; set; }

    public SpotifyPlaylistAlbum? Album { get; set; }
    public List<SpotifyPlaylistArtist> Artists { get; set; } = [];
}

public class SpotifyPlaylistAlbum
{
    public string? Id { get; set; }
    public string? Uri { get; set; }
    public string? Href { get; set; }
    public string? Name { get; set; }
    public List<SpotifyImage>? Images { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("external_urls")]
    public SpotifyPlaylistItemExternalUrls? ExternalUrls { get; set; }
}

public class SpotifyPlaylistArtist
{
    public string? Id { get; set; }
    public string? Uri { get; set; }
    public string? Href { get; set; }
    public string? Name { get; set; }

    [JsonPropertyName("external_urls")]
    public SpotifyPlaylistItemExternalUrls? ExternalUrls { get; set; }
}

public class SpotifyPlaylistItemExternalUrls
{
    public string? Spotify { get; set; }
}
