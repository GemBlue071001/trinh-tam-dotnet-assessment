using Domain.Entities;

namespace Application.Repositories;

public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart?> GetActiveByUserId(Guid userId);
}
