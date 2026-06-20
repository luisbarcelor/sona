using Sona.Application.DTOs;

namespace Sona.Application.Abstractions;

public interface ISpotifyProfileGateway
{
    Task<CurrentUserProfileDto> GetCurrentUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}
