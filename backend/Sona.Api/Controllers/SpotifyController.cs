using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sona.Application.Abstractions;
using Sona.Application.Auth;
using Sona.Application.Configuration;
using Sona.Application.Spotify;

namespace Sona.Api.Controllers;

[ApiController]
[Route("spotify")]
public class SpotifyController(
    SpotifyConnectionService connectionService,
    SpotifyAccountService accountService,
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
            return Redirect(connectionService.CreateAuthorizationUrl());
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
            var result = await connectionService.CompleteAuthorizationAsync(
                code,
                state,
                cancellationToken);

            Response.Cookies.Append(SessionCookieName, result.SessionId, CreateSessionCookieOptions());

            return RedirectToFrontend();
        }
        catch (SpotifyProviderException exception)
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
            connected = connectionService.IsConnected(GetSessionId())
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

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            var user = await accountService.GetCurrentUserProfileAsync(GetSessionId(), cancellationToken);

            return Ok(user);
        }
        catch (SpotifyConnectionRequiredException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
        catch (SpotifyProviderException exception)
        {
            if (exception.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
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
            var playlists = await accountService.GetCurrentUserPlaylistsAsync(
                GetSessionId(),
                limit,
                offset,
                cancellationToken);

            return Ok(playlists);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (SpotifyConnectionRequiredException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
        catch (SpotifyProviderException exception)
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

    [HttpGet("playlists/{playlistId}/items")]
    public async Task<IActionResult> GetPlaylistItems(
        [FromRoute] string playlistId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            var items = await accountService.GetPlaylistItemsAsync(
                GetSessionId(),
                playlistId,
                limit,
                offset,
                cancellationToken);

            return Ok(items);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (SpotifyConnectionRequiredException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
        catch (SpotifyProviderException exception)
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
        connectionService.Disconnect(GetSessionId());
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
