using System.Net;
using Sona.Infrastructure.Spotify.Models;
using Sona.Tests.Spotify.TestSupport;
using static Sona.Tests.Spotify.TestSupport.SpotifyControllerTestFactory;

namespace Sona.Tests.Spotify;

public class SpotifyAuthorizationServiceTests
{
    [Fact]
    public async Task GetAccessTokenAsync_WithExpiredToken_RefreshesAndPreservesSession()
    {
        var fixture = CreateFixture(authHandler: new DelegateHttpMessageHandler(_ =>
            JsonResponse(HttpStatusCode.OK, """
                {
                  "access_token": "new-access-token",
                  "token_type": "Bearer",
                  "expires_in": 3600,
                  "scope": "playlist-read-private user-read-private"
                }
                """)));
        var sessionId = fixture.TokenStore.SaveAuthorization(new SpotifyTokenResponse
        {
            AccessToken = "expired-access-token",
            RefreshToken = "refresh-token",
            ExpiresIn = -60,
            Scope = "playlist-read-private user-read-private",
            TokenType = "Bearer"
        });

        var accessToken = await fixture.AuthorizationService.GetAccessTokenAsync(sessionId);
        var storedToken = fixture.TokenStore.Get(sessionId);

        Assert.Equal("new-access-token", accessToken);
        Assert.NotNull(storedToken);
        Assert.Equal("refresh-token", storedToken.RefreshToken);
        Assert.Equal(sessionId, storedToken.SessionId);
    }
}
