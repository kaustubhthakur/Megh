
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateFileChunkDto
{
    [Required]
    public Guid StoredFileId { get; set; }

    [Range(0, int.MaxValue)]
    public int SequenceNumber { get; set; }

    [Range(1, long.MaxValue)]
    public long OriginalSizeBytes { get; set; }

    [Range(1, long.MaxValue)]
    public long EncryptedSizeBytes { get; set; }

    [Required]
    [StringLength(64, MinimumLength = 64)]
    public string ChunkHash { get; set; } = string.Empty;

    [Range(1, 10)]
    public int RequiredReplicaCount { get; set; } = 2;
}
