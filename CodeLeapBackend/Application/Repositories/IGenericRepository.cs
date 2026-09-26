using System.Linq.Expressions;

namespace Application.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<List<T>> GetAll();
    Task<List<T>> GetAllBy(Expression<Func<T, bool>> condition);
    Task<T?> GetById(Guid id);
    Task<T?> GetBy (Expression<Func<T, bool>> condition);
    Task Add(T entity);
    Task Update(T entity);
    Task Delete(T entity);
    Task<int> Save();
}
