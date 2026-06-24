using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Sona.Api.Controllers;
using Sona.Application.Auth;
using Sona.Application.Configuration;
using Sona.Application.Spotify;
using Sona.Infrastructure.Spotify.Api;
using Sona.Infrastructure.Spotify.Authorization;
using Sona.Infrastructure.Spotify.Models;

namespace Sona.Tests.Spotify.TestSupport;

internal static class SpotifyControllerTestFactory
{
    internal static TestFixture CreateFixture(
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
        var connectionService = new SpotifyConnectionService(authorizationService);
        var accountService = new SpotifyAccountService(
            authorizationService,
            new SpotifyProfileGateway(spotifyClient),
            new SpotifyPlaylistGateway(spotifyClient));
        var httpContext = new DefaultHttpContext();
        var controller = new SpotifyController(
            connectionService,
            accountService,
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
            connectionService,
            authorizationService,
            tokenStore,
            httpContext,
            httpContext.Response.Headers);
    }

    internal static HttpResponseMessage PlaylistDetailsResponse(
        string playlistId = "playlist-id",
        string snapshotId = "snapshot")
    {
        return JsonResponse(HttpStatusCode.OK, $$"""
            {
              "collaborative": false,
              "description": "Test playlist",
              "external_urls": { "spotify": "https://open.spotify.com/playlist/{{playlistId}}" },
              "href": "https://api.spotify.test/v1/playlists/{{playlistId}}",
              "id": "{{playlistId}}",
              "images": [],
              "name": "Editor playlist",
              "owner": {
                "external_urls": { "spotify": "https://open.spotify.com/user/user" },
                "href": "https://api.spotify.test/v1/users/user",
                "id": "user",
                "type": "user",
                "uri": "spotify:user:user",
                "display_name": "Tester"
              },
              "public": false,
              "snapshot_id": "{{snapshotId}}",
              "items": {
                "href": "https://api.spotify.test/v1/playlists/{{playlistId}}/tracks",
                "total": 51
              },
              "type": "playlist",
              "uri": "spotify:playlist:{{playlistId}}"
            }
            """);
    }

    internal static string TrackItemJson(string name, string id)
    {
        return $$"""
            {
              "added_at": "2026-06-20T09:00:00Z",
              "is_local": false,
              "item": {
                "duration_ms": 183000,
                "explicit": false,
                "external_urls": { "spotify": "https://open.spotify.com/track/{{id}}" },
                "href": "https://api.spotify.test/v1/tracks/{{id}}",
                "id": "{{id}}",
                "is_playable": true,
                "name": "{{name}}",
                "type": "track",
                "uri": "spotify:track:{{id}}"
              }
            }
            """;
    }

    internal static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    internal static string CreateValidState(SpotifyConnectionService connectionService)
    {
        var authorizationUrl = connectionService.CreateAuthorizationUrl();
        return ParseQuery(new Uri(authorizationUrl).Query)["state"];
    }

    internal static string SaveToken(DevelopmentSpotifyTokenStore tokenStore)
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

    internal static void SetCookie(HttpContext httpContext, string name, string value)
    {
        httpContext.Request.Headers.Cookie = $"{name}={value}";
    }

    internal static string GetSetCookieHeader(IHeaderDictionary headers)
    {
        Assert.True(headers.TryGetValue("Set-Cookie", out var values));
        return values.ToString();
    }

    internal static string GetCookieValue(IHeaderDictionary headers, string name)
    {
        var setCookie = GetSetCookieHeader(headers);
        var cookie = setCookie
            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .First(part => part.StartsWith($"{name}=", StringComparison.Ordinal));

        return cookie[(name.Length + 1)..];
    }

    internal static Dictionary<string, string> ParseQuery(string query)
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
}

internal sealed record TestFixture(
    SpotifyController Controller,
    SpotifyConnectionService ConnectionService,
    SpotifyAuthorizationService AuthorizationService,
    DevelopmentSpotifyTokenStore TokenStore,
    HttpContext HttpContext,
    IHeaderDictionary ResponseHeaders);

internal sealed class DelegateHttpMessageHandler(
    Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(send(request));
    }
}

internal sealed class TestEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "Sona.Tests";
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = string.Empty;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
