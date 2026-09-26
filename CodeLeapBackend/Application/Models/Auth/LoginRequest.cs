using System.ComponentModel.DataAnnotations;

namespace Application.Models.Auth;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = String.Empty;

    [Required]
    [StringLength(72, MinimumLength = 8)]
    public string Password { get; set; } = String.Empty;
}
