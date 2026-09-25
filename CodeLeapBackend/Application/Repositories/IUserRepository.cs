using Domain.Entities;

namespace Application.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<bool> IsEmailRegistered(string email);
}
