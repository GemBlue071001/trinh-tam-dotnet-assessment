using Application.Models.Auth;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;


namespace Application.Services.Auth;

public sealed class AuthService(IUserRepository userRepository, ILogger<AuthService> logger) : IAuthService
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
}
