using Application.Models.Auth;

namespace Application.Services.Auth;

public interface IAuthService
{
    Task<RegisterResponse?> Register(RegisterRequest registerRequest);
    Task<LoginResponse?> Login(LoginRequest loginRequest);
}
