using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string AuthCookieName = "__Host-megh-auth";

    private readonly AuthService _authService;
    private readonly IAntiforgery _antiforgery;

    public AuthController(
        AuthService authService,
        IAntiforgery antiforgery)
    {
        _authService = authService;
        _antiforgery = antiforgery;
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
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto)
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
                    fullName = result.FullName,
                    email = result.Email
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

 
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto)
    {
        try
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
                    fullName = result.FullName,
                    email = result.Email
                }
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var fullName = User.FindFirst(
            System.Security.Claims.ClaimTypes.Name)?.Value;

        var email = User.FindFirst(
            System.Security.Claims.ClaimTypes.Email)?.Value;

        return Ok(new
        {
            id = userId,
            fullName,
            email
        });
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
                MaxAge = TimeSpan.FromMinutes(30)
            });
    }
}