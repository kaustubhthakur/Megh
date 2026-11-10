
namespace backend.DTOs;

public class StoredFileResponseDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long OriginalSizeBytes { get; set; }

    public string EncryptionKeyId { get; set; } = string.Empty;

    public string FileHash { get; set; } = string.Empty;

    public int ChunkCount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
