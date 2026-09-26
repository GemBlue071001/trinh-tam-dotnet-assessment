using Application.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(ECommerceDbContext dbContext)
    : GenericRepository<Order>(dbContext), IOrderRepository
{
}
