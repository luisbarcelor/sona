using System.Text.Json.Serialization;

namespace Sona.Infrastructure.Spotify.Models;

public class SpotifyErrorResponse
{
    public SpotifyError? Error { get; set; }
}

public class SpotifyError
{
    public int Status { get; set; }
    public string? Message { get; set; }
}
