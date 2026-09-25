namespace Application.Models.Auth;

public sealed class RegisterResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
}
