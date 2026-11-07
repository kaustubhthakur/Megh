
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class StoredFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int UserId { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ContentType { get; set; }

    public long OriginalSizeBytes { get; set; }

    [Required, MaxLength(64)]
    public string EncryptionKeyId { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string FileHash { get; set; } = string.Empty;

    public int ChunkCount { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;

    public ICollection<FileChunk> Chunks { get; set; }
        = new List<FileChunk>();
}
