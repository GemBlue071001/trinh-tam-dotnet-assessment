namespace Application.Repositories;

public interface IUnitOfWork
{
    Task<T> ExecuteInTransaction<T>(Func<Task<T>> action);
}
