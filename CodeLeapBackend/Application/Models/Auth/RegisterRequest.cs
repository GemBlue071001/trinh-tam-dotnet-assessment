using System.ComponentModel.DataAnnotations;

namespace Application.Models.Auth;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(72, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [StringLength(150)]
    public string? FullName { get; set; }

    [StringLength(30)]
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}
