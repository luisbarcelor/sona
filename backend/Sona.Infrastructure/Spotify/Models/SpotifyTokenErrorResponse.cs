using System.Text.Json.Serialization;

namespace Sona.Infrastructure.Spotify.Models;

public class SpotifyTokenErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }
}
