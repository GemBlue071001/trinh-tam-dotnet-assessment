using System.Security.Cryptography;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class ECommerceDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        var now = DateTimeOffset.UtcNow;

        if (!await dbContext.Users.AnyAsync())
        {
            dbContext.Users.Add(CreateAdmin(now));
        }

        if (!await dbContext.Products.AnyAsync())
        {
            dbContext.Products.AddRange(
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "MacBook Air M3",
                    Description = "Laptop 13-inch for everyday work.",
                    Price = 1099.00m,
                    StockQuantity = 10,
                    Category = "Laptop",
                    ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Sony WH-1000XM5",
                    Description = "Wireless noise-cancelling headphones.",
                    Price = 349.00m,
                    StockQuantity = 20,
                    Category = "Headphones",
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Samsung 27-inch Monitor",
                    Description = "27-inch QHD monitor for productivity.",
                    Price = 299.00m,
                    StockQuantity = 15,
                    Category = "Monitor",
                    ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf",
                    CreatedAt = now,
                    UpdatedAt = now
                });
        }

        await dbContext.SaveChangesAsync();
    }

    private static User CreateAdmin(DateTimeOffset now)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var passwordHash = Rfc2898DeriveBytes.Pbkdf2(
            password: "Admin@123",
            salt: salt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);

        return new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@codeleap.local",
            PasswordSalt = Convert.ToBase64String(salt),
            PasswordHash = Convert.ToBase64String(passwordHash),
            Role = UserRole.Admin,
            IsActive = true,
            FullName = "System Admin",
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
