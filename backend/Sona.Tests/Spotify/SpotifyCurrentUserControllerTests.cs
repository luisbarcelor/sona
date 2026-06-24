using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sona.Application.DTOs;
using Sona.Tests.Spotify.TestSupport;
using static Sona.Tests.Spotify.TestSupport.SpotifyControllerTestFactory;

namespace Sona.Tests.Spotify;

public class SpotifyCurrentUserControllerTests
{
    [Fact]
    public async Task GetCurrentUser_WithValidSessionCookie_ReturnsProfile()
    {
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(request =>
        {
            Assert.Equal("/v1/me", request.RequestUri?.AbsolutePath);

            return JsonResponse(HttpStatusCode.OK, """
                {
                  "display_name": "Sona Tester",
                  "external_urls": { "spotify": "https://open.spotify.com/user/sona-tester" },
                  "href": "https://api.spotify.test/v1/users/sona-tester",
                  "id": "sona-tester",
                  "images": [
                    {
                      "url": "https://i.scdn.co/image/profile",
                      "height": 300,
                      "width": 300
                    }
                  ],
                  "type": "user",
                  "uri": "spotify:user:sona-tester"
                }
                """);
        }));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<OkObjectResult>(await fixture.Controller.GetCurrentUser());
        var profile = Assert.IsType<CurrentUserProfileDto>(result.Value);

        Assert.Equal("sona-tester", profile.Id);
        Assert.Equal("Sona Tester", profile.DisplayName);
        Assert.Single(profile.Images);
        Assert.Equal("https://i.scdn.co/image/profile", profile.Images[0].Url);
    }

    [Fact]
    public async Task GetCurrentUser_WhenSpotifyReturnsUnauthorized_ClearsSession()
    {
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(request =>
        {
            Assert.Equal("/v1/me", request.RequestUri?.AbsolutePath);

            return JsonResponse(HttpStatusCode.Unauthorized, """
                {
                  "error": {
                    "status": 401,
                    "message": "The access token expired"
                  }
                }
                """);
        }));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<ObjectResult>(await fixture.Controller.GetCurrentUser());

        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Null(fixture.TokenStore.Get(sessionId));
        Assert.Contains("sona_spotify_session=", GetSetCookieHeader(fixture.ResponseHeaders));
    }

    [Fact]
    public async Task GetCurrentUser_WhenSpotifyReturnsForbidden_ClearsSession()
    {
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(request =>
        {
            Assert.Equal("/v1/me", request.RequestUri?.AbsolutePath);

            return JsonResponse(HttpStatusCode.Forbidden, """
                {
                  "error": {
                    "status": 403,
                    "message": "Insufficient client scope"
                  }
                }
                """);
        }));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<ObjectResult>(await fixture.Controller.GetCurrentUser());

        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        Assert.Null(fixture.TokenStore.Get(sessionId));
        Assert.Contains("sona_spotify_session=", GetSetCookieHeader(fixture.ResponseHeaders));
    }
}
