using Sona.Application.Abstractions;
using Sona.Application.DTOs;
using Sona.Infrastructure.Spotify.Models;

namespace Sona.Infrastructure.Spotify.Api;

public class SpotifyProfileGateway(SpotifyClient spotifyClient) : ISpotifyProfileGateway
{
    public async Task<CurrentUserProfileDto> GetCurrentUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await spotifyClient.GetCurrentUserProfileAsync(accessToken, cancellationToken);

            return new CurrentUserProfileDto
            {
                Id = profile.Id,
                DisplayName = profile.DisplayName,
                Images = profile.Images.Select(MapImage).ToList()
            };
        }
        catch (SpotifyApiException exception)
        {
            throw new SpotifyProviderException(exception.StatusCode, exception.Message);
        }
    }

    private static ImageDto MapImage(SpotifyImage image)
    {
        return new ImageDto
        {
            Url = image.Url,
            Height = image.Height,
            Width = image.Width
        };
    }
}
