
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class FileChunk
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StoredFileId { get; set; }

    public int SequenceNumber { get; set; }

    public long OriginalSizeBytes { get; set; }

    public long EncryptedSizeBytes { get; set; }

    [Required, MaxLength(64)]
    public string ChunkHash { get; set; } = string.Empty;

    public int RequiredReplicaCount { get; set; } = 2;

    public string Status { get; set; } = "Pending";

    public StoredFile StoredFile { get; set; } = null!;

    public ICollection<ChunkReplica> Replicas { get; set; }
        = new List<ChunkReplica>();
}
