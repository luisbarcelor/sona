using System.Text.Json.Serialization;

namespace Sona.Application.DTOs;

public class PlaylistDto
{
    public required bool Collaborative { get; set; }
    public string? Description { get; set; }

    [JsonPropertyName("external_urls")]
    public required SpotifyExternalUrlsDto ExternalUrls { get; set; }

    public required string Href { get; set; }
    public required string Id { get; set; }
    public required List<ImageDto> Images { get; set; }
    public required string Name { get; set; }
    public required PlaylistOwnerDto Owner { get; set; }
    public bool? Public { get; set; }

    [JsonPropertyName("snapshot_id")]
    public required string SnapshotId { get; set; }

    public required PlaylistTracksReferenceDto Items { get; set; }
    public required string Type { get; set; }
    public required string Uri { get; set; }
}

public class SpotifyExternalUrlsDto
{
    public required string Spotify { get; set; }
}

public class PlaylistOwnerDto
{
    [JsonPropertyName("external_urls")]
    public required SpotifyExternalUrlsDto ExternalUrls { get; set; }

    public required string Href { get; set; }
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required string Uri { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}

public class PlaylistTracksReferenceDto
{
    public required string Href { get; set; }
    public required int Total { get; set; }
}
