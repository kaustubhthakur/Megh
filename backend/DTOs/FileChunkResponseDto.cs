
namespace backend.DTOs;

public class FileChunkResponseDto
{
    public Guid Id { get; set; }

    public Guid StoredFileId { get; set; }

    public int SequenceNumber { get; set; }

    public long OriginalSizeBytes { get; set; }

    public long EncryptedSizeBytes { get; set; }

    public string ChunkHash { get; set; } = string.Empty;

    public int RequiredReplicaCount { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ReplicaCount { get; set; }
}
