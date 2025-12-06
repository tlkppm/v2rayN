namespace ServiceLib.Repositories;

public class RoutingRepository : RepositoryBase<RoutingItem>
{
    private static readonly Lazy<RoutingRepository> _instance = new(() => new());
    public static RoutingRepository Instance => _instance.Value;

    public async Task<List<RoutingItem>> GetAllOrderedAsync()
    {
        return await SQLiteHelper.Instance.TableAsync<RoutingItem>().OrderBy(t => t.Sort).ToListAsync();
    }

    public new async Task<RoutingItem?> GetByIdAsync(string? id)
    {
        if (id.IsNullOrEmpty()) return null;
        return await FirstOrDefaultAsync(it => it.Id == id);
    }

    public async Task<RoutingItem?> GetActiveAsync()
    {
        return await FirstOrDefaultAsync(it => it.IsActive == true);
    }

    public async Task SetActiveAsync(string id)
    {
        var items = await GetAllAsync();
        foreach (var item in items)
        {
            item.IsActive = item.Id == id;
        }
        await UpdateAllAsync(items);
    }
}
