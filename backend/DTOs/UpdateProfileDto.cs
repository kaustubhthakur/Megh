
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class UpdateProfileDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(
        @"^[a-zA-Z0-9_]+$",
        ErrorMessage = "Username can contain only letters, numbers, and underscores.")]
    public string Username { get; set; } = string.Empty;
}
