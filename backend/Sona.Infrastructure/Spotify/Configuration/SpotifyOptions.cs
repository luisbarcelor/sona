namespace Sona.Infrastructure.Spotify.Configuration;

public class SpotifyOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string AccountsBaseUrl { get; set; } = string.Empty;

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string RedirectUri { get; set; } = string.Empty;

    public string FrontendUrl { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;
}
