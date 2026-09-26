using Application.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class RevokedAccessTokenRepository(ECommerceDbContext dbContext)
    : GenericRepository<RevokedAccessToken>(dbContext), IRevokedAccessTokenRepository
{
    public Task<bool> IsRevoked(string jwtId)
    {
        return DbSet.AnyAsync(token => token.JwtId == jwtId && token.ExpiresAt > DateTimeOffset.UtcNow);
    }
}
