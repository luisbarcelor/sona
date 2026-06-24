using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sona.Application.DTOs;
using Sona.Tests.Spotify.TestSupport;
using static Sona.Tests.Spotify.TestSupport.SpotifyControllerTestFactory;

namespace Sona.Tests.Spotify;

public class SpotifyPlaylistsControllerTests
{
    [Fact]
    public async Task GetPlaylists_WithoutSessionCookie_ReturnsUnauthorized()
    {
        var fixture = CreateFixture();

        var result = Assert.IsType<UnauthorizedObjectResult>(await fixture.Controller.GetPlaylists());

        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task GetPlaylists_WithValidSessionCookie_ReturnsPlaylists()
    {
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(_ =>
            JsonResponse(HttpStatusCode.OK, """
                {
                  "href": "https://api.spotify.test/v1/me/playlists",
                  "limit": 20,
                  "next": null,
                  "offset": 0,
                  "previous": null,
                  "total": 1,
                  "items": [
                    {
                      "collaborative": false,
                      "description": "Test playlist",
                      "external_urls": { "spotify": "https://open.spotify.com/playlist/abc" },
                      "href": "https://api.spotify.test/v1/playlists/abc",
                      "id": "abc",
                      "images": [],
                      "name": "Development Auth",
                      "owner": {
                        "external_urls": { "spotify": "https://open.spotify.com/user/user" },
                        "href": "https://api.spotify.test/v1/users/user",
                        "id": "user",
                        "type": "user",
                        "uri": "spotify:user:user",
                        "display_name": "Tester"
                      },
                      "public": false,
                      "snapshot_id": "snapshot",
                      "items": {
                        "href": "https://api.spotify.test/v1/playlists/abc/tracks",
                        "total": 12
                      },
                      "type": "playlist",
                      "uri": "spotify:playlist:abc"
                    }
                  ]
                }
                """)));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<OkObjectResult>(await fixture.Controller.GetPlaylists());
        var playlists = Assert.IsType<PagedResponseDto<PlaylistDto>>(result.Value);

        Assert.Single(playlists.Items);
        Assert.Equal("Development Auth", playlists.Items[0].Name);
    }

    [Fact]
    public async Task GetPlaylists_WhenSpotifyReturnsNullImages_MapsImagesToEmptyList()
    {
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(_ =>
            JsonResponse(HttpStatusCode.OK, """
                {
                  "href": "https://api.spotify.test/v1/me/playlists",
                  "limit": 20,
                  "next": null,
                  "offset": 0,
                  "previous": null,
                  "total": 1,
                  "items": [
                    {
                      "collaborative": false,
                      "description": null,
                      "external_urls": { "spotify": "https://open.spotify.com/playlist/empty" },
                      "href": "https://api.spotify.test/v1/playlists/empty",
                      "id": "empty",
                      "images": null,
                      "name": "Empty playlist",
                      "owner": {
                        "external_urls": { "spotify": "https://open.spotify.com/user/user" },
                        "href": "https://api.spotify.test/v1/users/user",
                        "id": "user",
                        "type": "user",
                        "uri": "spotify:user:user",
                        "display_name": "Tester"
                      },
                      "public": false,
                      "snapshot_id": "snapshot",
                      "items": {
                        "href": "https://api.spotify.test/v1/playlists/empty/tracks",
                        "total": 0
                      },
                      "type": "playlist",
                      "uri": "spotify:playlist:empty"
                    }
                  ]
                }
                """)));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<OkObjectResult>(await fixture.Controller.GetPlaylists());
        var playlists = Assert.IsType<PagedResponseDto<PlaylistDto>>(result.Value);

        Assert.Single(playlists.Items);
        Assert.Equal("Empty playlist", playlists.Items[0].Name);
        Assert.Empty(playlists.Items[0].Images);
        Assert.Equal(0, playlists.Items[0].Items.Total);
    }

    [Fact]
    public async Task GetPlaylists_WhenSpotifyReturnsUnauthorized_ClearsSession()
    {
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(_ =>
            JsonResponse(HttpStatusCode.Unauthorized, """
                {
                  "error": {
                    "status": 401,
                    "message": "The access token expired"
                  }
                }
                """)));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<ObjectResult>(await fixture.Controller.GetPlaylists());

        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Null(fixture.TokenStore.Get(sessionId));
        Assert.Contains("sona_spotify_session=", GetSetCookieHeader(fixture.ResponseHeaders));
    }
}
