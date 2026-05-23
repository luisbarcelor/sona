using Microsoft.AspNetCore.Mvc;
using Sona.Infrastructure.Spotify;
using Sona.Infrastructure.Spotify.Api;
using Sona.Infrastructure.Spotify.Authorization;

namespace Sona.Api.Controllers;

[ApiController]
[Route("spotify")]
public class SpotifyController(
    SpotifyClient spotifyClient,
    SpotifyAuthorizationService authorizationService,
    IWebHostEnvironment environment) : ControllerBase
{
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
            return BadRequest(new { message = $"Spotify authorization failed: {error}." });
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        {
            return BadRequest(new { message = "Spotify callback state or authorization code is invalid." });
        }

        try
        {
            var token = await authorizationService.CompleteAuthorizationAsync(
                code,
                state,
                cancellationToken);

            return Ok(new
            {
                message = "Spotify account connected. Call GET /spotify/playlists to verify the connection.",
                expiresIn = token.ExpiresIn,
                scope = token.Scope
            });
        }
        catch (SpotifyApiException exception)
        {
            return StatusCode((int)exception.StatusCode, new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
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
            var accessToken = await authorizationService.GetAccessTokenAsync(cancellationToken);

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
            return StatusCode((int)exception.StatusCode, new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
