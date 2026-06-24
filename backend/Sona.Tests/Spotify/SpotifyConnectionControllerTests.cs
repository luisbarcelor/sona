using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Sona.Tests.Spotify.TestSupport.SpotifyControllerTestFactory;

namespace Sona.Tests.Spotify;

public class SpotifyConnectionControllerTests
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
        var state = CreateValidState(fixture.ConnectionService);

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
}
