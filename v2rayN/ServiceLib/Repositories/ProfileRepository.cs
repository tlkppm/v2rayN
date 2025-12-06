namespace ServiceLib.Repositories;

public class ProfileRepository : RepositoryBase<ProfileItem>
{
    private static readonly Lazy<ProfileRepository> _instance = new(() => new());
    public static ProfileRepository Instance => _instance.Value;

    public async Task<ProfileItem?> GetByIndexIdAsync(string? indexId)
    {
        if (indexId.IsNullOrEmpty()) return null;
        return await FirstOrDefaultAsync(it => it.IndexId == indexId);
    }

    public async Task<ProfileItem?> GetByRemarksAsync(string? remarks)
    {
        if (remarks.IsNullOrEmpty()) return null;
        return await FirstOrDefaultAsync(it => it.Remarks == remarks);
    }

    public async Task<List<ProfileItem>> GetBySubIdAsync(string? subId)
    {
        if (subId.IsNullOrEmpty())
        {
            return await GetAllAsync();
        }
        return await WhereAsync(t => t.Subid == subId);
    }

    public async Task<List<ProfileItemModel>> QueryWithSubRemarksAsync(string subid, string filter)
    {
        var sql = @"select a.*, b.remarks subRemarks
                    from ProfileItem a
                    left join SubItem b on a.subid = b.id
                    where 1=1";
        var args = new List<object>();

        if (subid.IsNotEmpty())
        {
            sql += " and a.subid = ?";
            args.Add(subid);
        }
        if (filter.IsNotEmpty())
        {
            sql += " and (a.remarks like ? or a.address like ?)";
            var likePattern = $"%{filter}%";
            args.Add(likePattern);
            args.Add(likePattern);
        }

        return await SQLiteHelper.Instance.QueryAsync<ProfileItemModel>(sql, args.ToArray());
    }

    public async Task DeleteBySubIdAsync(string subId, bool isSub)
    {
        var items = isSub
            ? await WhereAsync(t => t.IsSub == true && t.Subid == subId)
            : await WhereAsync(t => t.Subid == subId);

        foreach (var item in items)
        {
            await DeleteAsync(item);
        }
    }
}
