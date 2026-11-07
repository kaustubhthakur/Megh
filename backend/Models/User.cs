
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(254)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string PasswordHash { get; set; } = string.Empty;

    public long StorageUsedBytes { get; set; } = 0;

    public long StorageLimitBytes { get; set; } = 1073741824;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ConnectedAccount> ConnectedAccounts { get; set; }
    = new List<ConnectedAccount>();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
