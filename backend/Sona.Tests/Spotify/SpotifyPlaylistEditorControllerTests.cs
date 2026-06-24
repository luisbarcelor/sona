using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sona.Application.DTOs;
using Sona.Tests.Spotify.TestSupport;
using static Sona.Tests.Spotify.TestSupport.SpotifyControllerTestFactory;

namespace Sona.Tests.Spotify;

public class SpotifyPlaylistEditorControllerTests
{
    [Fact]
    public async Task GetPlaylistEditor_ReturnsCombinedPlaylistItems()
    {
        var requestedOffsets = new List<string>();
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/v1/playlists/playlist-id")
            {
                return PlaylistDetailsResponse(snapshotId: "editor-snapshot");
            }

            Assert.Equal("/v1/playlists/playlist-id/items", request.RequestUri?.AbsolutePath);
            var query = ParseQuery(request.RequestUri?.Query ?? string.Empty);
            requestedOffsets.Add(query["offset"]);

            return query["offset"] switch
            {
                "0" => JsonResponse(HttpStatusCode.OK, """
                    {
                      "href": "https://api.spotify.test/v1/playlists/playlist-id/items",
                      "limit": 50,
                      "next": "https://api.spotify.test/v1/playlists/playlist-id/items?limit=50&offset=50",
                      "offset": 0,
                      "previous": null,
                      "total": 51,
                      "items": [
                """ + string.Join(",", Enumerable.Range(1, 50).Select(index => TrackItemJson($"Track {index}", $"track-{index}"))) + """
                      ]
                    }
                    """),
                "50" => JsonResponse(HttpStatusCode.OK, """
                    {
                      "href": "https://api.spotify.test/v1/playlists/playlist-id/items",
                      "limit": 50,
                      "next": null,
                      "offset": 50,
                      "previous": "https://api.spotify.test/v1/playlists/playlist-id/items?limit=50&offset=0",
                      "total": 51,
                      "items": [
                        {
                          "added_at": "2026-06-20T09:03:00Z",
                          "is_local": false,
                          "item": {
                            "duration_ms": 201000,
                            "explicit": false,
                            "external_urls": { "spotify": "https://open.spotify.com/track/final-track" },
                            "href": "https://api.spotify.test/v1/tracks/final-track",
                            "id": "track-51",
                            "is_playable": true,
                            "name": "Track 51",
                            "type": "track",
                            "uri": "spotify:track:track-51"
                          }
                        }
                      ]
                    }
                    """),
                _ => throw new InvalidOperationException($"Unexpected offset {query["offset"]}.")
            };
        }));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<OkObjectResult>(
            await fixture.Controller.GetPlaylistEditor("playlist-id"));
        var editor = Assert.IsType<PlaylistEditorDto>(result.Value);

        Assert.Equal(["0", "50"], requestedOffsets);
        Assert.Equal("playlist-id", editor.PlaylistId);
        Assert.Equal("editor-snapshot", editor.SnapshotId);
        Assert.Equal(51, editor.Items.Count);
        Assert.Equal(51, editor.Total);
        Assert.Equal("Track 1", editor.Items[0].Item?.Name);
        Assert.Equal("Track 51", editor.Items[50].Item?.Name);
    }

    [Fact]
    public async Task GetPlaylistEditor_WithoutSessionCookie_ReturnsUnauthorized()
    {
        var fixture = CreateFixture();

        var result = Assert.IsType<UnauthorizedObjectResult>(
            await fixture.Controller.GetPlaylistEditor("playlist-id"));

        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task GetPlaylistEditor_WithEmptyPlaylist_ReturnsEmptyEditor()
    {
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/v1/playlists/playlist-id")
            {
                return PlaylistDetailsResponse();
            }

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
                  "total": 0,
                  "items": []
                }
                """);
        }));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<OkObjectResult>(
            await fixture.Controller.GetPlaylistEditor("playlist-id"));
        var editor = Assert.IsType<PlaylistEditorDto>(result.Value);

        Assert.Equal("playlist-id", editor.PlaylistId);
        Assert.Equal(0, editor.Total);
        Assert.Empty(editor.Items);
    }

    [Fact]
    public async Task GetPlaylistEditor_WithSinglePage_ReturnsOnlyFirstPage()
    {
        var itemRequestCount = 0;
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/v1/playlists/playlist-id")
            {
                return PlaylistDetailsResponse();
            }

            itemRequestCount++;
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
                  "total": 1,
                  "items": [
                    {
                      "added_at": "2026-06-20T09:00:00Z",
                      "is_local": false,
                      "item": {
                        "duration_ms": 183000,
                        "explicit": false,
                        "external_urls": { "spotify": "https://open.spotify.com/track/only-track" },
                        "href": "https://api.spotify.test/v1/tracks/only-track",
                        "id": "only-track",
                        "is_playable": true,
                        "name": "Only Track",
                        "type": "track",
                        "uri": "spotify:track:only-track"
                      }
                    }
                  ]
                }
                """);
        }));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<OkObjectResult>(
            await fixture.Controller.GetPlaylistEditor("playlist-id"));
        var editor = Assert.IsType<PlaylistEditorDto>(result.Value);

        Assert.Equal(1, itemRequestCount);
        Assert.Equal(1, editor.Total);
        Assert.Single(editor.Items);
        Assert.Equal("Only Track", editor.Items[0].Item?.Name);
    }

    [Fact]
    public async Task GetPlaylistEditor_MapsUnsupportedItems()
    {
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/v1/playlists/playlist-id")
            {
                return PlaylistDetailsResponse();
            }

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
                    },
                    {
                      "added_at": null,
                      "is_local": false,
                      "item": null
                    }
                  ]
                }
                """);
        }));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<OkObjectResult>(
            await fixture.Controller.GetPlaylistEditor("playlist-id"));
        var editor = Assert.IsType<PlaylistEditorDto>(result.Value);

        Assert.Equal(2, editor.Items.Count);
        Assert.Null(editor.Items[0].Item);
        Assert.Equal("Only Spotify track items are supported in this editor.", editor.Items[0].UnsupportedReason);
        Assert.Null(editor.Items[1].Item);
        Assert.Equal("Spotify did not return details for this playlist item.", editor.Items[1].UnsupportedReason);
    }

    [Fact]
    public async Task GetPlaylistEditor_WhenLaterPageFails_ReturnsError()
    {
        var requestedOffsets = new List<string>();
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/v1/playlists/playlist-id")
            {
                return PlaylistDetailsResponse();
            }

            var query = ParseQuery(request.RequestUri?.Query ?? string.Empty);
            requestedOffsets.Add(query["offset"]);

            return query["offset"] == "0"
                ? JsonResponse(HttpStatusCode.OK, """
                    {
                      "href": "https://api.spotify.test/v1/playlists/playlist-id/items",
                      "limit": 50,
                      "next": "https://api.spotify.test/v1/playlists/playlist-id/items?limit=50&offset=50",
                      "offset": 0,
                      "previous": null,
                      "total": 51,
                      "items": []
                    }
                    """)
                : JsonResponse(HttpStatusCode.Forbidden, """
                    {
                      "error": {
                        "status": 403,
                        "message": "You cannot read this playlist"
                      }
                    }
                    """);
        }));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<ObjectResult>(
            await fixture.Controller.GetPlaylistEditor("playlist-id"));

        Assert.Equal(["0", "50"], requestedOffsets);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        Assert.NotNull(fixture.TokenStore.Get(sessionId));
    }

    [Fact]
    public async Task GetPlaylistEditor_WhenSuccessfulPagesAreIncomplete_ReturnsProblem()
    {
        var requestedOffsets = new List<string>();
        var fixture = CreateFixture(apiHandler: new DelegateHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/v1/playlists/playlist-id")
            {
                return PlaylistDetailsResponse();
            }

            var query = ParseQuery(request.RequestUri?.Query ?? string.Empty);
            requestedOffsets.Add(query["offset"]);

            return query["offset"] == "0"
                ? JsonResponse(HttpStatusCode.OK, """
                    {
                      "href": "https://api.spotify.test/v1/playlists/playlist-id/items",
                      "limit": 50,
                      "next": "https://api.spotify.test/v1/playlists/playlist-id/items?limit=50&offset=50",
                      "offset": 0,
                      "previous": null,
                      "total": 51,
                      "items": [
                        {
                          "added_at": "2026-06-20T09:00:00Z",
                          "is_local": false,
                          "item": {
                            "duration_ms": 183000,
                            "explicit": false,
                            "external_urls": { "spotify": "https://open.spotify.com/track/first-track" },
                            "href": "https://api.spotify.test/v1/tracks/first-track",
                            "id": "first-track",
                            "is_playable": true,
                            "name": "First Track",
                            "type": "track",
                            "uri": "spotify:track:first-track"
                          }
                        }
                      ]
                    }
                    """)
                : JsonResponse(HttpStatusCode.OK, """
                    {
                      "href": "https://api.spotify.test/v1/playlists/playlist-id/items",
                      "limit": 50,
                      "next": null,
                      "offset": 50,
                      "previous": "https://api.spotify.test/v1/playlists/playlist-id/items?limit=50&offset=0",
                      "total": 51,
                      "items": []
                    }
                    """);
        }));
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = Assert.IsType<ObjectResult>(
            await fixture.Controller.GetPlaylistEditor("playlist-id"));

        Assert.Equal(["0", "50"], requestedOffsets);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task GetPlaylistEditor_WhenSpotifyReturnsUnauthorized_ClearsSession()
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
            await fixture.Controller.GetPlaylistEditor("playlist-id"));

        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Null(fixture.TokenStore.Get(sessionId));
        Assert.Contains("sona_spotify_session=", GetSetCookieHeader(fixture.ResponseHeaders));
    }

    [Fact]
    public async Task GetPlaylistEditor_WhenSpotifyReturnsForbidden_PreservesSession()
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
            await fixture.Controller.GetPlaylistEditor("playlist-id"));

        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        Assert.NotNull(fixture.TokenStore.Get(sessionId));
    }
}
