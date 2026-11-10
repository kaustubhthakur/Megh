
using System.Security.Cryptography;
using backend.Services.CloudStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Controllers;

[ApiController]
[Route("api/google-drive")]
[Authorize]
public class GoogleDriveController : ControllerBase
{
    private readonly GoogleOAuthService _googleOAuth;
    private readonly IMemoryCache _cache;

    public GoogleDriveController(
        GoogleOAuthService googleOAuth,
        IMemoryCache cache)
    {
        _googleOAuth = googleOAuth;
        _cache = cache;
    }

    [HttpGet("connect")]
    public IActionResult Connect()
    {
        var userIdClaim =
            User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var state = WebEncoders.Base64UrlEncode(
            RandomNumberGenerator.GetBytes(32));

        _cache.Set(
            $"GoogleOAuthState:{state}",
            userId,
            TimeSpan.FromMinutes(10));

        return Redirect(
            _googleOAuth.GetAuthorizationUrl(state));
    }

    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            return BadRequest(new
            {
                message = "Google authorization was not completed.",
                error
            });
        }

        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(state))
        {
            return BadRequest(new
            {
                message = "Authorization code or state is missing."
            });
        }

        var cacheKey = $"GoogleOAuthState:{state}";

        if (!_cache.TryGetValue(cacheKey, out int userId))
        {
            return BadRequest(new
            {
                message = "Invalid or expired OAuth state. Please reconnect."
            });
        }

        _cache.Remove(cacheKey);

        try
        {
            var account =
                await _googleOAuth.CompleteAuthorizationAsync(
                    userId,
                    code,
                    cancellationToken);

            return Ok(new
            {
                message = "Google Drive connected successfully.",
                account.Id,
                account.Email,
                account.Provider,
                account.IsConnected
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                message = "Google authorization failed. Please try again."
            });
        }
    }

    [HttpGet("quota/{accountId:int}")]
    public async Task<IActionResult> GetQuota(
        int accountId,
        CancellationToken cancellationToken)
    {
        var userIdClaim =
            User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        try
        {
            var quota = await _googleOAuth.SyncQuotaAsync(
                userId,
                accountId,
                cancellationToken);

            return Ok(quota);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                message = "Connected Google account not found."
            });
        }
        catch (InvalidOperationException)
        {
            return BadRequest(new
            {
                message = "Unable to retrieve Google Drive quota. Reconnect if necessary."
            });
        }
    }
}
