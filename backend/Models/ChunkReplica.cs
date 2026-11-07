
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class ChunkReplica
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FileChunkId { get; set; }

    public int ConnectedAccountId { get; set; }

    [Required, MaxLength(1000)]
    public string ProviderFileId { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? VerifiedAt { get; set; }

    public FileChunk FileChunk { get; set; } = null!;

    public ConnectedAccount ConnectedAccount { get; set; } = null!;
}
