using System.Text.Json.Serialization;

namespace Sona.Infrastructure.Spotify.Models;

public class SpotifyPlaylist
{
    public required bool Collaborative { get; set; }
    public string? Description { get; set; }

    [JsonPropertyName("external_urls")]
    public required SpotifyExternalUrls ExternalUrls { get; set; }
    public required string Href { get; set; }
    public required string Id { get; set; }
    public required List<SpotifyImage> Images { get; set; }
    public required string Name { get; set; }
    public required SpotifyOwner Owner { get; set; }
    public bool? Public { get; set; }

    [JsonPropertyName("snapshot_id")]
    public required string SnapshotId { get; set; }
    public required SpotifyTracksReference Items { get; set; }
    public required string Type { get; set; }
    public required string Uri { get; set; }
}

public class SpotifyExternalUrls
{
    public required string Spotify { get; set; }
}

public class SpotifyImage
{
    public required string Url { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }
}

public class SpotifyOwner
{
    [JsonPropertyName("external_urls")]
    public required SpotifyExternalUrls ExternalUrls { get; set; }
    public required string Href { get; set; }
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required string Uri { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}

public class SpotifyTracksReference
{
    public required string Href { get; set; }
    public required int Total { get; set; }
}
