using System.Linq.Expressions;

namespace ServiceLib.Repositories;

public interface IRepository<T> where T : class, new()
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate);
    Task<int> InsertAsync(T entity);
    Task<int> InsertAllAsync(IEnumerable<T> entities);
    Task<int> UpdateAsync(T entity);
    Task<int> UpdateAllAsync(IEnumerable<T> entities);
    Task<int> DeleteAsync(T entity);
    Task<int> ReplaceAsync(T entity);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
}
