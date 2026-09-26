using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Configurations;
using Application.Models.Auth;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;


namespace Application.Services.Auth;

public sealed class AuthService(
    IUserRepository userRepository,
    IRevokedAccessTokenRepository revokedAccessTokenRepository,
    ILogger<AuthService> logger,
    EnvSettings envSettings) : IAuthService
{
    public async Task<RegisterResponse?> Register(RegisterRequest registerRequest)
    {
        var email = registerRequest.Email.Trim();

        if (await userRepository.IsEmailRegistered(email))
        {
            logger.LogError("Attempted registration with already registered email: {Email}", email);
            return null;
        }

        var now = DateTimeOffset.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
            Role = UserRole.Customer,
            IsActive = true,
            FullName = registerRequest.FullName?.Trim(),
            PhoneNumber = registerRequest.PhoneNumber?.Trim(),
            Address = registerRequest.Address?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await userRepository.Add(user);
        await userRepository.Save();

        return new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email
        };
    }

    public async Task<LoginResponse?> Login(LoginRequest loginRequest)
    {
        var user = await userRepository.GetBy(x => x.Email == loginRequest.Email.Trim());
        if (user is null || !user.IsActive)
        {
            logger.LogWarning("Failed login attempt for email: {Email}", loginRequest.Email);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
        {
            logger.LogWarning("Failed login attempt for email: {Email}", loginRequest.Email);
            return null;
        }

        return new LoginResponse
        {
            JwtToken = GenerateJwtToken(user)
        };
    }

    public async Task<CurrentUserResponse?> GetCurrentUser(Guid userId)
    {
        var user = await userRepository.GetById(userId);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        return new CurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address
        };
    }

    public async Task Logout(string jwtId, DateTimeOffset expiresAt)
    {
        var existingToken = await revokedAccessTokenRepository.GetBy(token => token.JwtId == jwtId);
        if (existingToken is not null)
        {
            return;
        }

        await revokedAccessTokenRepository.Add(new RevokedAccessToken
        {
            Id = Guid.NewGuid(),
            JwtId = jwtId,
            ExpiresAt = expiresAt,
            RevokedAt = DateTimeOffset.UtcNow
        });
        await revokedAccessTokenRepository.Save();
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = envSettings.Jwt;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
