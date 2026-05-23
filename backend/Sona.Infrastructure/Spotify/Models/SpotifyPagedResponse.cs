using System.Text.Json.Serialization;

namespace Sona.Infrastructure.Spotify.Models;

public class SpotifyPagedResponse<T>
{
    public required string Href { get; set; }
    public required int Limit { get; set; }
    public string? Next { get; set; }
    public required int Offset { get; set; }
    public string? Previous { get; set; }
    public required int Total { get; set; }
    public required List<T> Items { get; set; }
}
