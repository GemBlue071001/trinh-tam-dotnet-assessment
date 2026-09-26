using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class CartRepository(ECommerceDbContext dbContext)
    : GenericRepository<Cart>(dbContext), ICartRepository
{
    public Task<Cart?> GetActiveByUserId(Guid userId)
    {
        return DbSet
            .Include(cart => cart.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(cart => cart.UserId == userId && cart.Status == CartStatus.Active);
    }

    public Task<Cart?> GetActiveForCheckout(Guid userId)
    {
        return DbSet
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.UserId == userId && cart.Status == CartStatus.Active);
    }
}
