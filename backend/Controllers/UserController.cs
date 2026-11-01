
using backend.Data;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public UserController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET: api/user
    // Get all users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .Select(u => new
            {
                id = u.Id,
                name = u.Name,
                username = u.Username,
                email = u.Email,
                storageUsedBytes = u.StorageUsedBytes,
                storageLimitBytes = u.StorageLimitBytes
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET: api/user/5
    // Get a user by ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
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
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(user);
    }

    // PUT: api/user/profile
    // Update the authenticated user's profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileDto dto)
    {
        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Invalid user authentication."
            });
        }

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        var name = dto.Name?.Trim();
        var username = dto.Username?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message = "Name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new
            {
                message = "Username is required."
            });
        }

        // Check whether another user already uses this username.
        var usernameExists = await _dbContext.Users
            .AnyAsync(u =>
                u.Id != userId &&
                u.Username == username);

        if (usernameExists)
        {
            return Conflict(new
            {
                message = "Username is already taken."
            });
        }

        user.Name = name;
        user.Username = username;

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Profile updated successfully.",
            user = new
            {
                id = user.Id,
                name = user.Name,
                username = user.Username,
                email = user.Email,
                storageUsedBytes = user.StorageUsedBytes,
                storageLimitBytes = user.StorageLimitBytes
            }
        });
    }
}
