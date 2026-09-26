using System.Data;
using Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class UnitOfWork(ECommerceDbContext dbContext) : IUnitOfWork
{
    public async Task<T> ExecuteInTransaction<T>(Func<Task<T>> action)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var result = await action();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
