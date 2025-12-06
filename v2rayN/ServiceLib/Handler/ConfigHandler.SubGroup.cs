namespace ServiceLib.Handler;

public static partial class ConfigHandler
{
    #region Sub & Group

    public static async Task<int> AddSubItem(Config config, string url)
    {
        var count = await SQLiteHelper.Instance.TableAsync<SubItem>().CountAsync(e => e.Url == url);
        if (count > 0)
        {
            return 0;
        }
        SubItem subItem = new()
        {
            Id = string.Empty,
            Url = url
        };

        var uri = Utils.TryUri(url);
        if (uri == null)
        {
            return -1;
        }
        if (url.StartsWith(Global.HttpProtocol) && !Utils.IsPrivateNetwork(uri.IdnHost))
        {
            NoticeManager.Instance.Enqueue(ResUI.InsecureUrlProtocol);
        }

        var queryVars = Utils.ParseQueryString(uri.Query);
        subItem.Remarks = queryVars["remarks"] ?? "import_sub";

        return await AddSubItem(config, subItem);
    }

    public static async Task<int> AddSubItem(Config config, SubItem subItem)
    {
        var item = await AppManager.Instance.GetSubItem(subItem.Id);
        if (item is null)
        {
            item = subItem;
        }
        else
        {
            item.Remarks = subItem.Remarks;
            item.Url = subItem.Url;
            item.MoreUrl = subItem.MoreUrl;
            item.Enabled = subItem.Enabled;
            item.AutoUpdateInterval = subItem.AutoUpdateInterval;
            item.UserAgent = subItem.UserAgent;
            item.Sort = subItem.Sort;
            item.Filter = subItem.Filter;
            item.UpdateTime = subItem.UpdateTime;
            item.ConvertTarget = subItem.ConvertTarget;
            item.PrevProfile = subItem.PrevProfile;
            item.NextProfile = subItem.NextProfile;
            item.PreSocksPort = subItem.PreSocksPort;
            item.Memo = subItem.Memo;
        }

        if (item.Id.IsNullOrEmpty())
        {
            item.Id = Utils.GetGuid(false);

            if (item.Sort <= 0)
            {
                var maxSort = 0;
                if (await SQLiteHelper.Instance.TableAsync<SubItem>().CountAsync() > 0)
                {
                    var lstSubs = await AppManager.Instance.SubItems();
                    maxSort = lstSubs.LastOrDefault()?.Sort ?? 0;
                }
                item.Sort = maxSort + 1;
            }
        }
        if (await SQLiteHelper.Instance.ReplaceAsync(item) > 0)
        {
            return 0;
        }
        return -1;
    }

    public static async Task<int> RemoveServersViaSubid(Config config, string subid, bool isSub)
    {
        if (subid.IsNullOrEmpty())
        {
            return -1;
        }
        var customProfile = await SQLiteHelper.Instance.TableAsync<ProfileItem>()
            .Where(t => t.Subid == subid && t.ConfigType == EConfigType.Custom).ToListAsync();

        if (isSub)
        {
            var items = await SQLiteHelper.Instance.TableAsync<ProfileItem>()
                .Where(t => t.IsSub == true && t.Subid == subid).ToListAsync();
            foreach (var item in items)
            {
                await SQLiteHelper.Instance.DeleteAsync(item);
            }
        }
        else
        {
            var items = await SQLiteHelper.Instance.TableAsync<ProfileItem>()
                .Where(t => t.Subid == subid).ToListAsync();
            foreach (var item in items)
            {
                await SQLiteHelper.Instance.DeleteAsync(item);
            }
        }

        foreach (var item in customProfile)
        {
            File.Delete(Utils.GetConfigPath(item.Address));
        }

        return 0;
    }

    public static async Task<int> DeleteSubItem(Config config, string id)
    {
        var item = await AppManager.Instance.GetSubItem(id);
        if (item is null)
        {
            return 0;
        }
        await SQLiteHelper.Instance.DeleteAsync(item);
        await RemoveServersViaSubid(config, id, false);

        return 0;
    }

    public static async Task<int> MoveToGroup(Config config, List<ProfileItem> lstProfile, string subid)
    {
        foreach (var item in lstProfile)
        {
            item.Subid = subid;
        }
        await SQLiteHelper.Instance.UpdateAllAsync(lstProfile);

        return 0;
    }

    #endregion Sub & Group
}
