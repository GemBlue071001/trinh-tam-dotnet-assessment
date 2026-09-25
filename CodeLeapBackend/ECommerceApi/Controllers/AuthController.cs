using Application.Models.Auth;
using Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> Register(RegisterRequest registerRequest, CancellationToken cancellationToken)
    {
        await authService.Register(registerRequest, cancellationToken);

        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
