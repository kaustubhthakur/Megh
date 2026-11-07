
namespace backend.Models;

public class UploadSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int UserId { get; set; }

    public Guid StoredFileId { get; set; }

    public string Status { get; set; } = "Started";

    public int CompletedChunks { get; set; }

    public int TotalChunks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
