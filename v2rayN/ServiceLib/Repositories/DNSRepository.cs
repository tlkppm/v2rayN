namespace ServiceLib.Repositories;

public class DNSRepository : RepositoryBase<DNSItem>
{
    private static readonly Lazy<DNSRepository> _instance = new(() => new());
    public static DNSRepository Instance => _instance.Value;

    public async Task<DNSItem?> GetByCoreTypeAsync(ECoreType coreType)
    {
        return await FirstOrDefaultAsync(it => it.CoreType == coreType);
    }

    public async Task DeleteAllAsync()
    {
        var items = await GetAllAsync();
        foreach (var item in items)
        {
            await DeleteAsync(item);
        }
    }
}

public class FullConfigTemplateRepository : RepositoryBase<FullConfigTemplateItem>
{
    private static readonly Lazy<FullConfigTemplateRepository> _instance = new(() => new());
    public static FullConfigTemplateRepository Instance => _instance.Value;

    public async Task<FullConfigTemplateItem?> GetByCoreTypeAsync(ECoreType coreType)
    {
        return await FirstOrDefaultAsync(it => it.CoreType == coreType);
    }
}

public class ProfileGroupRepository : RepositoryBase<ProfileGroupItem>
{
    private static readonly Lazy<ProfileGroupRepository> _instance = new(() => new());
    public static ProfileGroupRepository Instance => _instance.Value;

    public async Task<ProfileGroupItem?> GetByIndexIdAsync(string? indexId)
    {
        if (indexId.IsNullOrEmpty()) return null;
        return await FirstOrDefaultAsync(it => it.IndexId == indexId);
    }
}

public class ServerStatRepository : RepositoryBase<ServerStatItem>
{
    private static readonly Lazy<ServerStatRepository> _instance = new(() => new());
    public static ServerStatRepository Instance => _instance.Value;

    public async Task<ServerStatItem?> GetByIndexIdAsync(string indexId)
    {
        return await FirstOrDefaultAsync(it => it.IndexId == indexId);
    }
}
