using System.Net;

namespace Sona.Application.Abstractions;

public class SpotifyProviderException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
