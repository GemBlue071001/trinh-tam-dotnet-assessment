using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using Application.Configurations;
using Application.Models.Auth;
using Application.Repositories;
using Application.Services.Auth;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.Services.Auth;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task Register_WhenEmailIsAvailable_CreatesCustomerWithHashedPassword()
    {
        var userRepository = new Mock<IUserRepository>();
        var revokedTokenRepository = new Mock<IRevokedAccessTokenRepository>();
        User? addedUser = null;
        userRepository.Setup(repo => repo.IsEmailRegistered("customer@example.com")).ReturnsAsync(false);
        userRepository.Setup(repo => repo.Add(It.IsAny<User>()))
            .Callback<User>(user => addedUser = user)
            .Returns(Task.CompletedTask);
        userRepository.Setup(repo => repo.Save()).ReturnsAsync(1);
        var service = CreateService(userRepository, revokedTokenRepository);

        var result = await service.Register(new RegisterRequest
        {
            Email = "  customer@example.com  ",
            Password = "Password@123",
            FullName = "  Customer Name  ",
            PhoneNumber = "  0900000000  ",
            Address = "  Ho Chi Minh City  "
        });

        Assert.NotNull(result);
        Assert.NotNull(addedUser);
        Assert.Equal("customer@example.com", addedUser!.Email);
        Assert.Equal(UserRole.Customer, addedUser.Role);
        Assert.True(addedUser.IsActive);
        Assert.Equal("Customer Name", addedUser.FullName);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password@123", addedUser.PasswordHash));
        Assert.Equal(addedUser.Id, result!.Id);
        userRepository.Verify(repo => repo.Save(), Times.Once);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ReturnsNullWithoutPersisting()
    {
        var userRepository = new Mock<IUserRepository>();
        var revokedTokenRepository = new Mock<IRevokedAccessTokenRepository>();
        userRepository.Setup(repo => repo.IsEmailRegistered("customer@example.com")).ReturnsAsync(true);
        var service = CreateService(userRepository, revokedTokenRepository);

        var result = await service.Register(new RegisterRequest
        {
            Email = "customer@example.com",
            Password = "Password@123"
        });

        Assert.Null(result);
        userRepository.Verify(repo => repo.Add(It.IsAny<User>()), Times.Never);
        userRepository.Verify(repo => repo.Save(), Times.Never);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsJwtWithUserClaims()
    {
        var user = CreateUser();
        var userRepository = new Mock<IUserRepository>();
        var revokedTokenRepository = new Mock<IRevokedAccessTokenRepository>();
        userRepository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<User, bool>>>() ))
            .ReturnsAsync(user);
        var service = CreateService(userRepository, revokedTokenRepository);

        var result = await service.Login(new LoginRequest
        {
            Email = user.Email,
            Password = "Password@123"
        });

        Assert.NotNull(result);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result!.JwtToken);
        Assert.Equal("unit-test-issuer", jwt.Issuer);
        Assert.Equal("unit-test-audience", jwt.Audiences.Single());
        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(claim => claim.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(UserRole.Admin.ToString(), jwt.Claims.Single(claim => claim.Type == ClaimTypes.Role).Value);
        Assert.False(string.IsNullOrWhiteSpace(jwt.Id));
    }

    [Theory]
    [InlineData(false, "Password@123")]
    [InlineData(true, "WrongPassword")]
    public async Task Login_WhenUserIsInactiveOrPasswordIsInvalid_ReturnsNull(bool isActive, string password)
    {
        var user = CreateUser();
        user.IsActive = isActive;
        var userRepository = new Mock<IUserRepository>();
        var revokedTokenRepository = new Mock<IRevokedAccessTokenRepository>();
        userRepository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<User, bool>>>() ))
            .ReturnsAsync(user);
        var service = CreateService(userRepository, revokedTokenRepository);

        var result = await service.Login(new LoginRequest { Email = user.Email, Password = password });

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_WhenUserDoesNotExist_ReturnsNull()
    {
        var userRepository = new Mock<IUserRepository>();
        var revokedTokenRepository = new Mock<IRevokedAccessTokenRepository>();
        userRepository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<User, bool>>>() ))
            .ReturnsAsync((User?)null);
        var service = CreateService(userRepository, revokedTokenRepository);

        var result = await service.Login(new LoginRequest { Email = "missing@example.com", Password = "Password@123" });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserIsActive_ReturnsProfile()
    {
        var user = CreateUser();
        var userRepository = new Mock<IUserRepository>();
        var revokedTokenRepository = new Mock<IRevokedAccessTokenRepository>();
        userRepository.Setup(repo => repo.GetById(user.Id)).ReturnsAsync(user);
        var service = CreateService(userRepository, revokedTokenRepository);

        var result = await service.GetCurrentUser(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal("Admin", result.Role);
        Assert.Equal(user.FullName, result.FullName);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetCurrentUser_WhenUserDoesNotExistOrIsInactive_ReturnsNull(bool userExists)
    {
        var userId = Guid.NewGuid();
        var userRepository = new Mock<IUserRepository>();
        var revokedTokenRepository = new Mock<IRevokedAccessTokenRepository>();
        var user = CreateUser();
        user.Id = userId;
        user.IsActive = false;
        userRepository.Setup(repo => repo.GetById(userId))
            .ReturnsAsync(userExists ? user : null);
        var service = CreateService(userRepository, revokedTokenRepository);

        var result = await service.GetCurrentUser(userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task Logout_WhenTokenIsNotYetRevoked_AddsRevokedToken()
    {
        var userRepository = new Mock<IUserRepository>();
        var revokedTokenRepository = new Mock<IRevokedAccessTokenRepository>();
        RevokedAccessToken? addedToken = null;
        revokedTokenRepository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<RevokedAccessToken, bool>>>() ))
            .ReturnsAsync((RevokedAccessToken?)null);
        revokedTokenRepository.Setup(repo => repo.Add(It.IsAny<RevokedAccessToken>()))
            .Callback<RevokedAccessToken>(token => addedToken = token)
            .Returns(Task.CompletedTask);
        revokedTokenRepository.Setup(repo => repo.Save()).ReturnsAsync(1);
        var service = CreateService(userRepository, revokedTokenRepository);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        await service.Logout("jwt-id", expiresAt);

        Assert.NotNull(addedToken);
        Assert.Equal("jwt-id", addedToken!.JwtId);
        Assert.Equal(expiresAt, addedToken.ExpiresAt);
        Assert.True(addedToken.RevokedAt <= DateTimeOffset.UtcNow);
        revokedTokenRepository.Verify(repo => repo.Save(), Times.Once);
    }

    [Fact]
    public async Task Logout_WhenTokenIsAlreadyRevoked_DoesNotPersistAgain()
    {
        var userRepository = new Mock<IUserRepository>();
        var revokedTokenRepository = new Mock<IRevokedAccessTokenRepository>();
        revokedTokenRepository.Setup(repo => repo.GetBy(It.IsAny<Expression<Func<RevokedAccessToken, bool>>>() ))
            .ReturnsAsync(new RevokedAccessToken { Id = Guid.NewGuid(), JwtId = "jwt-id" });
        var service = CreateService(userRepository, revokedTokenRepository);

        await service.Logout("jwt-id", DateTimeOffset.UtcNow.AddMinutes(30));

        revokedTokenRepository.Verify(repo => repo.Add(It.IsAny<RevokedAccessToken>()), Times.Never);
        revokedTokenRepository.Verify(repo => repo.Save(), Times.Never);
    }

    private static AuthService CreateService(
        Mock<IUserRepository> userRepository,
        Mock<IRevokedAccessTokenRepository> revokedTokenRepository)
    {
        return new AuthService(
            userRepository.Object,
            revokedTokenRepository.Object,
            new Mock<ILogger<AuthService>>().Object,
            new EnvSettings
            {
                Jwt = new JwtSettings
                {
                    Issuer = "unit-test-issuer",
                    Audience = "unit-test-audience",
                    Key = "unit-test-signing-key-that-is-long-enough-for-hmac-sha256",
                    AccessTokenExpirationMinutes = 60
                }
            });
    }

    private static User CreateUser()
    {
        var now = DateTimeOffset.UtcNow;
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
            Role = UserRole.Admin,
            IsActive = true,
            FullName = "Admin User",
            PhoneNumber = "0900000000",
            Address = "Ho Chi Minh City",
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
