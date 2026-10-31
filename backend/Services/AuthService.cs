using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public sealed record AuthResult(
    string Token,
    int UserId,
    string FullName,
    string Email
);

public class AuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext db,
        IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }


    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();


        if (Encoding.UTF8.GetByteCount(dto.Password) > 72)
        {
            throw new ArgumentException(
                "Password must not exceed 72 UTF-8 bytes.");
        }

        var existingUser = await _db.Users
            .AnyAsync(u => u.Email == email);

        if (existingUser)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        // Hash the password before saving it.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(
            dto.Password,
            workFactor: 12);

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
      
            throw new InvalidOperationException(
                "Unable to register this account. The email may already be registered.");
        }

        return GenerateToken(user);
    }

   
    public async Task<AuthResult?> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

 
        if (Encoding.UTF8.GetByteCount(dto.Password) > 72)
        {
            return null;
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email);



        if (user is null)
        {
            return null;
        }

        bool passwordIsValid;

        try
        {
            passwordIsValid = BCrypt.Net.BCrypt.Verify(
                dto.Password,
                user.PasswordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            passwordIsValid = false;
        }

        if (!passwordIsValid)
        {
            return null;
        }

        return GenerateToken(user);
    }

    private AuthResult GenerateToken(User user)
    {
        var keyString = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT signing key is not configured.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        byte[] keyBytes;

        try
        {
            keyBytes = Convert.FromBase64String(keyString);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                "JWT signing key must be Base64 encoded.");
        }

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key must contain at least 32 bytes.");
        }

        var expiresMinutes = 30;

        if (int.TryParse(
                _configuration["Jwt:ExpiresMinutes"],
                out var configuredMinutes))
        {
            expiresMinutes = configuredMinutes;
        }

        if (expiresMinutes < 1 || expiresMinutes > 60)
        {
            throw new InvalidOperationException(
                "Jwt:ExpiresMinutes must be between 1 and 60.");
        }

        var signingKey = new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.FullName),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return new AuthResult(
            tokenString,
            user.Id,
            user.FullName,
            user.Email);
    }
}