using System.Linq.Expressions;

namespace ServiceLib.Repositories;

public class RepositoryBase<T> : IRepository<T> where T : class, new()
{
    public virtual async Task<List<T>> GetAllAsync()
    {
        return await SQLiteHelper.Instance.TableAsync<T>().ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await SQLiteHelper.Instance.TableAsync<T>().FirstOrDefaultAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await SQLiteHelper.Instance.TableAsync<T>().FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate)
    {
        return await SQLiteHelper.Instance.TableAsync<T>().Where(predicate).ToListAsync();
    }

    public virtual async Task<int> InsertAsync(T entity)
    {
        return await SQLiteHelper.Instance.InsertAsync(entity);
    }

    public virtual async Task<int> InsertAllAsync(IEnumerable<T> entities)
    {
        return await SQLiteHelper.Instance.InsertAllAsync(entities);
    }

    public virtual async Task<int> UpdateAsync(T entity)
    {
        return await SQLiteHelper.Instance.UpdateAsync(entity);
    }

    public virtual async Task<int> UpdateAllAsync(IEnumerable<T> entities)
    {
        return await SQLiteHelper.Instance.UpdateAllAsync(entities);
    }

    public virtual async Task<int> DeleteAsync(T entity)
    {
        return await SQLiteHelper.Instance.DeleteAsync(entity);
    }

    public virtual async Task<int> ReplaceAsync(T entity)
    {
        return await SQLiteHelper.Instance.ReplaceAsync(entity);
    }

    public virtual async Task<int> CountAsync()
    {
        return await SQLiteHelper.Instance.TableAsync<T>().CountAsync();
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await SQLiteHelper.Instance.TableAsync<T>().CountAsync(predicate);
    }
}
