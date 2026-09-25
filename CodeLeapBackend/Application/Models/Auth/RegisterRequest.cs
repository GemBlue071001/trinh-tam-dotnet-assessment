namespace Application.Models.Auth;

public sealed class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}
