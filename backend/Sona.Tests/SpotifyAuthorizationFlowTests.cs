using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Sona.Api.Controllers;
using Sona.Infrastructure.Spotify;
using Sona.Infrastructure.Spotify.Api;
using Sona.Infrastructure.Spotify.Authorization;
using Sona.Infrastructure.Spotify.Configuration;
using Sona.Infrastructure.Spotify.Models;

namespace Sona.Tests;

public class SpotifyAuthorizationFlowTests
{
    [Fact]
    public void Connect_RedirectsToSpotifyAuthorizationUrl()
    {
        var fixture = CreateFixture();

        var result = Assert.IsType<RedirectResult>(fixture.Controller.Connect());
        var uri = new Uri(result.Url!);
        var query = ParseQuery(uri.Query);

        Assert.Equal("https", uri.Scheme);
        Assert.Equal("accounts.spotify.test", uri.Host);
        Assert.Equal("/authorize", uri.AbsolutePath);
        Assert.Equal("code", query["response_type"]);
        Assert.Equal("client-id", query["client_id"]);
        Assert.Equal("playlist-read-private user-read-private", query["scope"]);
        Assert.Equal("https://127.0.0.1:7001/spotify/callback", query["redirect_uri"]);
        Assert.False(string.IsNullOrWhiteSpace(query["state"]));
    }

    [Fact]
    public async Task Callback_WithInvalidState_RedirectsToFrontendError()
    {
        var fixture = CreateFixture();

        var result = Assert.IsType<RedirectResult>(await fixture.Controller.Callback(
            "authorization-code",
            "invalid-state",
            null));

        Assert.StartsWith("http://127.0.0.1:5173?spotify_error=", result.Url);
        Assert.False(fixture.ResponseHeaders.TryGetValue("Set-Cookie", out _));
    }

    [Fact]
    public async Task Callback_WithValidState_SetsSessionCookieAndRedirectsToFrontend()
    {
        var fixture = CreateFixture();
        var state = CreateValidState(fixture.AuthorizationService);

        var result = Assert.IsType<RedirectResult>(await fixture.Controller.Callback(
            "authorization-code",
            state,
            null));

        Assert.Equal("http://127.0.0.1:5173?spotify=connected", result.Url);

        var sessionId = GetCookieValue(fixture.ResponseHeaders, "sona_spotify_session");
        Assert.False(string.IsNullOrWhiteSpace(sessionId));
        Assert.Contains("httponly", GetSetCookieHeader(fixture.ResponseHeaders).ToLowerInvariant());
        Assert.NotNull(fixture.TokenStore.Get(sessionId));
    }

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
        var profile = Assert.IsType<SpotifyCurrentUser>(result.Value);

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
        var playlists = Assert.IsType<SpotifyPagedResponse<SpotifyPlaylist>>(result.Value);

        Assert.Single(playlists.Items);
        Assert.Equal("Development Auth", playlists.Items[0].Name);
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

    [Fact]
    public void Disconnect_ClearsSession()
    {
        var fixture = CreateFixture();
        var sessionId = SaveToken(fixture.TokenStore);
        SetCookie(fixture.HttpContext, "sona_spotify_session", sessionId);

        var result = fixture.Controller.Disconnect();

        Assert.IsType<NoContentResult>(result);
        Assert.Null(fixture.TokenStore.Get(sessionId));
        Assert.Contains("sona_spotify_session=", GetSetCookieHeader(fixture.ResponseHeaders));
    }

    private static TestFixture CreateFixture(
        HttpMessageHandler? authHandler = null,
        HttpMessageHandler? apiHandler = null)
    {
        var options = Options.Create(new SpotifyOptions
        {
            BaseUrl = "https://api.spotify.test",
            AccountsBaseUrl = "https://accounts.spotify.test",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            RedirectUri = "https://127.0.0.1:7001/spotify/callback",
            FrontendUrl = "http://127.0.0.1:5173",
            Scope = "playlist-read-private user-read-private"
        });

        var tokenStore = new DevelopmentSpotifyTokenStore();
        var authClient = new SpotifyAuthClient(new HttpClient(authHandler ?? TokenSuccessHandler())
        {
            BaseAddress = new Uri(options.Value.AccountsBaseUrl)
        });
        var authorizationService = new SpotifyAuthorizationService(authClient, tokenStore, options);
        var spotifyClient = new SpotifyClient(new HttpClient(apiHandler ?? EmptyPlaylistHandler())
        {
            BaseAddress = new Uri(options.Value.BaseUrl)
        });
        var httpContext = new DefaultHttpContext();
        var controller = new SpotifyController(
            spotifyClient,
            authorizationService,
            new TestEnvironment(),
            options)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        return new TestFixture(
            controller,
            authorizationService,
            tokenStore,
            httpContext,
            httpContext.Response.Headers);
    }

    private static HttpMessageHandler TokenSuccessHandler()
    {
        return new DelegateHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, """
            {
              "access_token": "access-token",
              "refresh_token": "refresh-token",
              "token_type": "Bearer",
              "expires_in": 3600,
              "scope": "playlist-read-private user-read-private"
            }
            """));
    }

    private static HttpMessageHandler EmptyPlaylistHandler()
    {
        return new DelegateHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, """
            {
              "href": "https://api.spotify.test/v1/me/playlists",
              "limit": 20,
              "next": null,
              "offset": 0,
              "previous": null,
              "total": 0,
              "items": []
            }
            """));
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static string CreateValidState(SpotifyAuthorizationService authorizationService)
    {
        var authorizationUrl = authorizationService.CreateAuthorizationUrl();
        return ParseQuery(new Uri(authorizationUrl).Query)["state"];
    }

    private static string SaveToken(DevelopmentSpotifyTokenStore tokenStore)
    {
        return tokenStore.SaveAuthorization(new SpotifyTokenResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresIn = 3600,
            Scope = "playlist-read-private user-read-private",
            TokenType = "Bearer"
        });
    }

    private static void SetCookie(HttpContext httpContext, string name, string value)
    {
        httpContext.Request.Headers.Cookie = $"{name}={value}";
    }

    private static string GetSetCookieHeader(IHeaderDictionary headers)
    {
        Assert.True(headers.TryGetValue("Set-Cookie", out var values));
        return values.ToString();
    }

    private static string GetCookieValue(IHeaderDictionary headers, string name)
    {
        var setCookie = GetSetCookieHeader(headers);
        var cookie = setCookie
            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .First(part => part.StartsWith($"{name}=", StringComparison.Ordinal));

        return cookie[(name.Length + 1)..];
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        return query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(
                parts => Uri.UnescapeDataString(parts[0].Replace("+", " ", StringComparison.Ordinal)),
                parts => parts.Length > 1
                    ? Uri.UnescapeDataString(parts[1].Replace("+", " ", StringComparison.Ordinal))
                    : string.Empty);
    }

    private sealed record TestFixture(
        SpotifyController Controller,
        SpotifyAuthorizationService AuthorizationService,
        DevelopmentSpotifyTokenStore TokenStore,
        HttpContext HttpContext,
        IHeaderDictionary ResponseHeaders);

    private sealed class DelegateHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(send(request));
        }
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Sona.Tests";
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
