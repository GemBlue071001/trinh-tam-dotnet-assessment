using Application.Models.Auth;

namespace Application.Services.Auth;

public sealed class AuthService : IAuthService
{
    public Task Register(RegisterRequest registerRequest, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
