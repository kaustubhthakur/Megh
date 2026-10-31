
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public record AuthResult(
    string Token,
    int UserId,
    string Name,
    string Username,
    string Email,
    long StorageUsedBytes,
    long StorageLimitBytes
);

public class AuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        var name = dto.Name.Trim();
        var username = dto.Username.Trim().ToLowerInvariant();
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _dbContext.Users.AnyAsync(u => u.Email == email))
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        if (await _dbContext.Users.AnyAsync(u => u.Username == username))
        {
            throw new InvalidOperationException("Username is already taken.");
        }

        var user = new User
        {
            Name = name,
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12),
            StorageUsedBytes = 0,
            StorageLimitBytes = 1073741824,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return CreateAuthResult(user);
    }

    public async Task<AuthResult?> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            return null;
        }

        if (Encoding.UTF8.GetByteCount(dto.Password) > 72)
        {
            return null;
        }

        try
        {
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return null;
            }
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return null;
        }

        return CreateAuthResult(user);
    }

    private AuthResult CreateAuthResult(User user)
    {
        var token = GenerateJwtToken(user);

        return new AuthResult(
            token,
            user.Id,
            user.Name,
            user.Username,
            user.Email,
            user.StorageUsedBytes,
            user.StorageLimitBytes
        );
    }

    private string GenerateJwtToken(User user)
    {
        var keyValue = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");

        byte[] keyBytes;

        try
        {
            keyBytes = Convert.FromBase64String(keyValue);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("JWT signing key must be valid Base64.");
        }

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("JWT signing key must contain at least 32 bytes.");
        }

        var expiresMinutes = _configuration.GetValue<int?>("Jwt:ExpiresMinutes") ?? 30;

        if (expiresMinutes is < 1 or > 60)
        {
            throw new InvalidOperationException("JWT expiration must be between 1 and 60 minutes.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
