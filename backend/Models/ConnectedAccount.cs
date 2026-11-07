
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class ConnectedAccount
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Provider { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProviderUserId { get; set; }

    [Required]
    public string EncryptedAccessToken { get; set; } = string.Empty;

    public string? EncryptedRefreshToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }

    public long? TotalBytes { get; set; }

    public long? UsedBytes { get; set; }

    public long? AvailableBytes { get; set; }

    public bool IsConnected { get; set; } = true;

    public DateTime? LastSyncedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
