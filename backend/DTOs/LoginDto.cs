using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}