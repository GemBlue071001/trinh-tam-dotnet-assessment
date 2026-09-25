using Application.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ECommerceDbContext dbContext) : GenericRepository<User>(dbContext), IUserRepository
{
    public Task<bool> IsEmailRegistered(string email)
    {
        return DbContext.Users.AnyAsync(user => user.Email == email);
    }
}
