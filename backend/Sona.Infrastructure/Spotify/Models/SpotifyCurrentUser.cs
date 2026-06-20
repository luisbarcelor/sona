using System.Text.Json.Serialization;

namespace Sona.Infrastructure.Spotify.Models;

public class SpotifyCurrentUser
{
    public required string Id { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    public required List<SpotifyImage> Images { get; set; }
}
