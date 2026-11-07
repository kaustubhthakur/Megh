using backend.Data;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IAntiforgery _antiforgery;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;

    private string AuthCookieName =>
        _configuration["Auth:CookieName"] ?? "__Host-megh-auth";

    private int CookieMaxAgeMinutes =>
        _configuration.GetValue<int?>("Auth:CookieMaxAgeMinutes") ?? 30;

    public AuthController(
        AuthService authService,
        IAntiforgery antiforgery,
        IConfiguration configuration,
        ApplicationDbContext dbContext)
    {
        _authService = authService;
        _antiforgery = antiforgery;
        _configuration = configuration;
        _dbContext = dbContext;
    }

    [HttpGet("csrf")]
    public IActionResult GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            csrfToken = tokens.RequestToken
        });
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);

            SetAuthCookie(result.Token);

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Registration successful.",
                user = new
                {
                    id = result.UserId,
                    name = result.Name,
                    username = result.Username,
                    email = result.Email,
                    storageUsedBytes = result.StorageUsedBytes,
                    storageLimitBytes = result.StorageLimitBytes
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        SetAuthCookie(result.Token);

        return Ok(new
        {
            message = "Login successful.",
            user = new
            {
                id = result.UserId,
                name = result.Name,
                username = result.Username,
                email = result.Email,
                storageUsedBytes = result.StorageUsedBytes,
                storageLimitBytes = result.StorageLimitBytes
            }
        });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                id = u.Id,
                name = u.Name,
                username = u.Username,
                email = u.Email,
                storageUsedBytes = u.StorageUsedBytes,
                storageLimitBytes = u.StorageLimitBytes
            })
            .SingleOrDefaultAsync();

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(
            AuthCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

        return Ok(new
        {
            message = "Logout successful."
        });
    }

    private void SetAuthCookie(string token)
    {
        Response.Cookies.Append(
            AuthCookieName,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                IsEssential = true,
                MaxAge = TimeSpan.FromMinutes(CookieMaxAgeMinutes)
            });
    }
}