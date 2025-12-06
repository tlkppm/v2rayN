namespace ServiceLib.Handler;

public static partial class ConfigHandler
{
    #region DNS

    public static async Task<int> InitBuiltinDNS(Config config)
    {
        var items = await AppManager.Instance.DNSItems();

        var needsUpdate = false;
        foreach (var existingItem in items)
        {
            if (existingItem.NormalDNS.IsNullOrEmpty() && existingItem.Enabled)
            {
                existingItem.Enabled = false;
                needsUpdate = true;
            }
        }

        if (needsUpdate)
        {
            await SQLiteHelper.Instance.UpdateAllAsync(items);
        }

        if (items.Count <= 0)
        {
            var item = new DNSItem()
            {
                Remarks = "V2ray",
                CoreType = ECoreType.Xray,
                Enabled = false,
            };
            await SaveDNSItems(config, item);

            var item2 = new DNSItem()
            {
                Remarks = "sing-box",
                CoreType = ECoreType.sing_box,
                Enabled = false,
            };
            await SaveDNSItems(config, item2);
        }

        return 0;
    }

    public static async Task<int> SaveDNSItems(Config config, DNSItem item)
    {
        if (item == null)
        {
            return -1;
        }

        if (item.Id.IsNullOrEmpty())
        {
            item.Id = Utils.GetGuid(false);
        }

        if (await SQLiteHelper.Instance.ReplaceAsync(item) > 0)
        {
            return 0;
        }
        return -1;
    }

    public static async Task<DNSItem> GetExternalDNSItem(ECoreType type, string url)
    {
        var currentItem = await AppManager.Instance.GetDNSItem(type);

        var downloadHandle = new DownloadService();
        var templateContent = await downloadHandle.TryDownloadString(url, true, "");
        if (templateContent.IsNullOrEmpty())
        {
            return currentItem;
        }

        var template = JsonUtils.Deserialize<DNSItem>(templateContent);
        if (template == null)
        {
            return currentItem;
        }

        if (!template.NormalDNS.IsNullOrEmpty())
        {
            template.NormalDNS = await downloadHandle.TryDownloadString(template.NormalDNS, true, "");
        }

        if (!template.TunDNS.IsNullOrEmpty())
        {
            template.TunDNS = await downloadHandle.TryDownloadString(template.TunDNS, true, "");
        }

        template.Id = currentItem.Id;
        template.Enabled = currentItem.Enabled;
        template.Remarks = currentItem.Remarks;
        template.CoreType = type;

        return template;
    }

    #endregion DNS

    #region Simple DNS

    public static SimpleDNSItem InitBuiltinSimpleDNS()
    {
        return new SimpleDNSItem()
        {
            UseSystemHosts = false,
            AddCommonHosts = true,
            FakeIP = false,
            GlobalFakeIp = true,
            BlockBindingQuery = true,
            DirectDNS = Global.DomainDirectDNSAddress.FirstOrDefault(),
            RemoteDNS = Global.DomainRemoteDNSAddress.FirstOrDefault(),
            BootstrapDNS = Global.DomainPureIPDNSAddress.FirstOrDefault(),
        };
    }

    public static async Task<SimpleDNSItem> GetExternalSimpleDNSItem(string url)
    {
        var downloadHandle = new DownloadService();
        var templateContent = await downloadHandle.TryDownloadString(url, true, "");
        if (templateContent.IsNullOrEmpty())
        {
            return null;
        }

        var template = JsonUtils.Deserialize<SimpleDNSItem>(templateContent);
        return template;
    }

    #endregion Simple DNS
}
