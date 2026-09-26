using Application.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class CartItemRepository(ECommerceDbContext dbContext)
    : GenericRepository<CartItem>(dbContext), ICartItemRepository
{
}
