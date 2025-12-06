namespace ServiceLib.Repositories;

public class SubItemRepository : RepositoryBase<SubItem>
{
    private static readonly Lazy<SubItemRepository> _instance = new(() => new());
    public static SubItemRepository Instance => _instance.Value;

    public async Task<List<SubItem>> GetAllOrderedAsync()
    {
        return await SQLiteHelper.Instance.TableAsync<SubItem>().OrderBy(t => t.Sort).ToListAsync();
    }

    public new async Task<SubItem?> GetByIdAsync(string? id)
    {
        if (id.IsNullOrEmpty()) return null;
        return await FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<SubItem?> GetByUrlAsync(string url)
    {
        return await FirstOrDefaultAsync(t => t.Url == url);
    }

    public async Task<int> GetMaxSortAsync()
    {
        var items = await GetAllOrderedAsync();
        return items.LastOrDefault()?.Sort ?? 0;
    }
}
