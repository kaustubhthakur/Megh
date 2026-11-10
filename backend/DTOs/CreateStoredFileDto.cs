
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateStoredFileDto
{
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ContentType { get; set; }

    [Range(1, long.MaxValue)]
    public long OriginalSizeBytes { get; set; }

    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string EncryptionKeyId { get; set; } = string.Empty;

    [Required]
    [StringLength(64, MinimumLength = 64)]
    public string FileHash { get; set; } = string.Empty;
}
