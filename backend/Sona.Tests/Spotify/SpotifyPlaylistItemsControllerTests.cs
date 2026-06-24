using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sona.Application.DTOs;
using Sona.Tests.Spotify.TestSupport;
using static Sona.Tests.Spotify.TestSupport.SpotifyControllerTestFactory;

namespace Sona.Tests.Spotify;

public class SpotifyPlaylistItemsControllerTests
{
    [Fact]
    public async Task GetPlaylistItems_WithoutSessionCookie_ReturnsUnauthorized()
    {
        var fixture = CreateFixture();

        var result = Assert.IsType<UnauthorizedObjectResult>(
            await fixture.Controller.GetPlaylistItems("playlist-id"));

        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task GetPlaylistItems_WithValidSessionCookie_ReturnsPlaylistItems()
    {
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(request =>
        {
            Assert.Equal("/v1/playlists/playlist-id/items", request.RequestUri?.AbsolutePath);
            var query = ParseQuery(request.RequestUri?.Query ?? string.Empty);
            Assert.Equal("50", query["limit"]);
            Assert.Equal("0", query["offset"]);

            return JsonResponse(HttpStatusCode.OK, """
                {
                  "href": "https://api.spotify.test/v1/playlists/playlist-id/items",
                  "limit": 50,
                  "next": null,
                  "offset": 0,
                  "previous": null,
                  "total": 2,
                  "items": [
                    {
                      "added_at": "2026-06-20T09:00:00Z",
                      "is_local": false,
                      "item": {
                        "album": {
                          "external_urls": { "spotify": "https://open.spotify.com/album/album-id" },
                          "href": "https://api.spotify.test/v1/albums/album-id",
                          "id": "album-id",
                          "images": [
                            {
                              "url": "https://i.scdn.co/image/album",
                              "height": 640,
                              "width": 640
                            }
                          ],
                          "name": "Album Name",
                          "release_date": "2026-01-01",
                          "type": "album",
                          "uri": "spotify:album:album-id"
                        },
                        "artists": [
                          {
                            "external_urls": { "spotify": "https://open.spotify.com/artist/artist-id" },
                            "href": "https://api.spotify.test/v1/artists/artist-id",
                            "id": "artist-id",
                            "name": "Artist Name",
                            "type": "artist",
                            "uri": "spotify:artist:artist-id"
                          }
                        ],
                        "duration_ms": 183000,
                        "explicit": false,
                        "external_urls": { "spotify": "https://open.spotify.com/track/track-id" },
                        "href": "https://api.spotify.test/v1/tracks/track-id",
                        "id": "track-id",
                        "is_playable": true,
                        "name": "Track Name",
                        "type": "track",
                        "uri": "spotify:track:track-id"
                      }
                    },
                    {
                      "added_at": null,
                      "is_local": false,
                      "item": {
                        "duration_ms": 240000,
                        "explicit": false,
                        "external_urls": { "spotify": "https://open.spotify.com/episode/episode-id" },
                        "href": "https://api.spotify.test/v1/episodes/episode-id",
                        "id": "episode-id",
                        "is_playable": true,
                        "name": "Episode Name",
                        "type": "episode",
                        "uri": "spotify:episode:episode-id"
                      }
                    }
                  ]
                }
                """);
        }));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<OkObjectResult>(
            await fixture.Controller.GetPlaylistItems("playlist-id"));
        var page = Assert.IsType<PagedResponseDto<PlaylistItemDto>>(result.Value);

        Assert.Equal(2, page.Items.Count);
        Assert.Equal("Track Name", page.Items[0].Item?.Name);
        Assert.Equal("Artist Name", page.Items[0].Item?.Artists[0].Name);
        Assert.Equal("Album Name", page.Items[0].Item?.Album?.Name);
        Assert.Null(page.Items[0].UnsupportedReason);
        Assert.Null(page.Items[1].Item);
        Assert.Equal("Only Spotify track items are supported in this editor.", page.Items[1].UnsupportedReason);
    }

    [Fact]
    public async Task GetPlaylistItems_WhenSpotifyReturnsUnauthorized_ClearsSession()
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

        var result = Assert.IsType<ObjectResult>(
            await fixture.Controller.GetPlaylistItems("playlist-id"));

        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Null(fixture.TokenStore.Get(sessionId));
        Assert.Contains("sona_spotify_session=", GetSetCookieHeader(fixture.ResponseHeaders));
    }

    [Fact]
    public async Task GetPlaylistItems_WhenSpotifyReturnsForbidden_PreservesSession()
    {
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(_ =>
            JsonResponse(HttpStatusCode.Forbidden, """
                {
                  "error": {
                    "status": 403,
                    "message": "You cannot read this playlist"
                  }
                }
                """)));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<ObjectResult>(
            await fixture.Controller.GetPlaylistItems("playlist-id"));

        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        Assert.NotNull(fixture.TokenStore.Get(sessionId));
    }
}
