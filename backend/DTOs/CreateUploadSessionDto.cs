
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateUploadSessionDto
{
    [Required]
    public Guid StoredFileId { get; set; }

    [Range(1, 100000)]
    public int TotalChunks { get; set; }
}
