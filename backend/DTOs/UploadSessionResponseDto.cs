
namespace backend.DTOs;

public class UploadSessionResponseDto
{
    public Guid Id { get; set; }

    public Guid StoredFileId { get; set; }

    public string Status { get; set; } = string.Empty;

    public int CompletedChunks { get; set; }

    public int TotalChunks { get; set; }

    public double ProgressPercentage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
