using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sona.Infrastructure.Spotify;
using Sona.Infrastructure.Spotify.Api;
using Sona.Infrastructure.Spotify.Authorization;
using Sona.Infrastructure.Spotify.Configuration;

namespace Sona.Api.Controllers;

[ApiController]
[Route("spotify")]
public class SpotifyController(
    SpotifyClient spotifyClient,
    SpotifyAuthorizationService authorizationService,
    IWebHostEnvironment environment,
    IOptions<SpotifyOptions> options) : ControllerBase
{
    private const string SessionCookieName = "sona_spotify_session";

    [HttpGet("connect")]
    public IActionResult Connect()
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            return Redirect(authorizationService.CreateAuthorizationUrl());
        }
        catch (InvalidOperationException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        if (error is not null)
        {
            return RedirectToFrontend(error);
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        {
            return RedirectToFrontend("invalid_callback");
        }

        try
        {
            var result = await authorizationService.CompleteAuthorizationAsync(
                code,
                state,
                cancellationToken);

            Response.Cookies.Append(SessionCookieName, result.SessionId, CreateSessionCookieOptions());

            return RedirectToFrontend();
        }
        catch (SpotifyApiException exception)
        {
            return RedirectToFrontend(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return RedirectToFrontend(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return RedirectToFrontend(exception.Message);
        }
    }

    [HttpGet("connection")]
    public IActionResult GetConnection()
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        return Ok(new
        {
            connected = authorizationService.IsConnected(GetSessionId())
        });
    }

    [HttpDelete("connection")]
    public IActionResult Disconnect()
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        ClearConnection();

        return NoContent();
    }

    [HttpGet("playlists")]
    public async Task<IActionResult> GetPlaylists(
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            var accessToken = await authorizationService.GetAccessTokenAsync(GetSessionId(), cancellationToken);

            if (accessToken is null)
            {
                return Unauthorized(new
                {
                    message = "Spotify is not connected. Open GET /spotify/connect first."
                });
            }

            var playlists = await spotifyClient.GetCurrentUserPlaylistsAsync(
                accessToken,
                limit,
                offset,
                cancellationToken);

            return Ok(playlists);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (SpotifyApiException exception)
        {
            if (exception.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ClearConnection();
            }

            return StatusCode((int)exception.StatusCode, new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private string? GetSessionId()
    {
        return Request.Cookies.TryGetValue(SessionCookieName, out var sessionId)
            ? sessionId
            : null;
    }

    private static CookieOptions CreateSessionCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = false
        };
    }

    private void ClearConnection()
    {
        authorizationService.Disconnect(GetSessionId());
        Response.Cookies.Delete(SessionCookieName, CreateSessionCookieOptions());
    }

    private IActionResult RedirectToFrontend(string? error = null)
    {
        var frontendUrl = options.Value.FrontendUrl;

        if (string.IsNullOrWhiteSpace(error))
        {
            return Redirect($"{frontendUrl.TrimEnd('/')}?spotify=connected");
        }

        return Redirect($"{frontendUrl.TrimEnd('/')}?spotify_error={Uri.EscapeDataString(error)}");
    }
}
