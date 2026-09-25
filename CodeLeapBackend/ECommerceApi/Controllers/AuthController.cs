using Application.Models.Auth;
using Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        var response = await authService.Register(registerRequest);

        if (response is null)
        {
            return Conflict(new { message = "Email is already registered." });
        }

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        var response = await authService.Login(loginRequest);

        if (response is null)
        {
            return BadRequest(new { message = "Email or password is incorrect" });
        }

        return Ok(response);
    }
}
