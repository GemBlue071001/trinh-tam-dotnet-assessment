using System.ComponentModel.DataAnnotations;

namespace Application.Models.Auth;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [StringLength(150)]
    public string? FullName { get; set; }

    [StringLength(30)]
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}
