using System.Linq.Expressions;
using Application.Exceptions;
using Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ECommerceDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(ECommerceDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public Task<List<T>> GetAll()
    {
        return DbSet.ToListAsync();
    }

    public async Task<T?> GetById(Guid id)
    {
        return await DbSet.FindAsync(id);
    }

    public async Task<T?> GetBy (Expression<Func<T, bool>> condition) 
    {
         return await DbSet.FirstOrDefaultAsync(condition);
    }

    public async Task Add(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    public async Task Update(T entity)
    {
        DbSet.Update(entity);
    }

    public async Task Delete(T entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<int> Save()
    {
        try
        {
            return await DbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            DbContext.ChangeTracker.Clear();
            throw new ConcurrencyConflictException();
        }
    }
}
