namespace Sona.Infrastructure.Spotify.Configuration;

public class SpotifyOptions
{
    public string BaseUrl { get; set; } = "https://api.spotify.com";

    public string AccountsBaseUrl { get; set; } = "https://accounts.spotify.com";

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string RedirectUri { get; set; } = "http://127.0.0.1:5000/spotify/callback";

    public string Scope { get; set; } = "playlist-read-private";
}
