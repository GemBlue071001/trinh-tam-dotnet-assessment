using Application.Models.Auth;

namespace Application.Services.Auth;

public interface IAuthService
{
    public Task Register(RegisterRequest registerRequest, CancellationToken cancellationToken = default);
}
