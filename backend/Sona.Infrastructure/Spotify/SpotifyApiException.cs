using System.Net;

namespace Sona.Infrastructure.Spotify;

public class SpotifyApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
