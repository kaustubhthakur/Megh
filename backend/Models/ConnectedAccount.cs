
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class ConnectedAccount
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = "Google";

    [MaxLength(500)]
    public string? ProviderUserId { get; set; }

    [Required]
    public string AccessToken { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }

    public long StorageUsedBytes { get; set; }

    public long? StorageLimitBytes { get; set; }

    public bool IsConnected { get; set; } = true;

    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
