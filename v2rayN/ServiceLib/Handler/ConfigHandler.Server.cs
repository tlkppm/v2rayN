namespace ServiceLib.Handler;

public static partial class ConfigHandler
{
    #region Server

    public static async Task<int> AddServer(Config config, ProfileItem profileItem)
    {
        var item = await AppManager.Instance.GetProfileItem(profileItem.IndexId);
        if (item is null)
        {
            item = profileItem;
        }
        else
        {
            item.CoreType = profileItem.CoreType;
            item.Remarks = profileItem.Remarks;
            item.Address = profileItem.Address;
            item.Port = profileItem.Port;
            item.Ports = profileItem.Ports;

            item.Id = profileItem.Id;
            item.AlterId = profileItem.AlterId;
            item.Security = profileItem.Security;
            item.Flow = profileItem.Flow;

            item.Network = profileItem.Network;
            item.HeaderType = profileItem.HeaderType;
            item.RequestHost = profileItem.RequestHost;
            item.Path = profileItem.Path;

            item.StreamSecurity = profileItem.StreamSecurity;
            item.Sni = profileItem.Sni;
            item.AllowInsecure = profileItem.AllowInsecure;
            item.Fingerprint = profileItem.Fingerprint;
            item.Alpn = profileItem.Alpn;

            item.PublicKey = profileItem.PublicKey;
            item.ShortId = profileItem.ShortId;
            item.SpiderX = profileItem.SpiderX;
            item.Mldsa65Verify = profileItem.Mldsa65Verify;
            item.Extra = profileItem.Extra;
            item.MuxEnabled = profileItem.MuxEnabled;
            item.Cert = profileItem.Cert;
        }

        var ret = item.ConfigType switch
        {
            EConfigType.VMess => await AddVMessServer(config, item),
            EConfigType.Shadowsocks => await AddShadowsocksServer(config, item),
            EConfigType.SOCKS => await AddSocksServer(config, item),
            EConfigType.HTTP => await AddHttpServer(config, item),
            EConfigType.Trojan => await AddTrojanServer(config, item),
            EConfigType.VLESS => await AddVlessServer(config, item),
            EConfigType.Hysteria2 => await AddHysteria2Server(config, item),
            EConfigType.TUIC => await AddTuicServer(config, item),
            EConfigType.WireGuard => await AddWireguardServer(config, item),
            EConfigType.Anytls => await AddAnytlsServer(config, item),
            _ => -1,
        };
        return ret;
    }

    public static async Task<int> AddVMessServer(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigType = EConfigType.VMess;

        profileItem.Address = profileItem.Address.TrimEx();
        profileItem.Id = profileItem.Id.TrimEx();
        profileItem.Security = profileItem.Security.TrimEx();
        profileItem.Network = profileItem.Network.TrimEx();
        profileItem.HeaderType = profileItem.HeaderType.TrimEx();
        profileItem.RequestHost = profileItem.RequestHost.TrimEx();
        profileItem.Path = profileItem.Path.TrimEx();
        profileItem.StreamSecurity = profileItem.StreamSecurity.TrimEx();

        if (!Global.VmessSecurities.Contains(profileItem.Security))
        {
            return -1;
        }
        if (profileItem.Id.IsNullOrEmpty())
        {
            return -1;
        }

        await AddServerCommon(config, profileItem, toFile);

        return 0;
    }

    public static async Task<int> AddShadowsocksServer(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigType = EConfigType.Shadowsocks;

        profileItem.Address = profileItem.Address.TrimEx();
        profileItem.Id = profileItem.Id.TrimEx();
        profileItem.Security = profileItem.Security.TrimEx();

        if (!AppManager.Instance.GetShadowsocksSecurities(profileItem).Contains(profileItem.Security))
        {
            return -1;
        }
        if (profileItem.Id.IsNullOrEmpty())
        {
            return -1;
        }

        await AddServerCommon(config, profileItem, toFile);

        return 0;
    }

    public static async Task<int> AddSocksServer(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigType = EConfigType.SOCKS;

        profileItem.Address = profileItem.Address.TrimEx();

        await AddServerCommon(config, profileItem, toFile);

        return 0;
    }

    public static async Task<int> AddHttpServer(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigType = EConfigType.HTTP;

        profileItem.Address = profileItem.Address.TrimEx();

        await AddServerCommon(config, profileItem, toFile);

        return 0;
    }

    public static async Task<int> AddTrojanServer(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigType = EConfigType.Trojan;

        profileItem.Address = profileItem.Address.TrimEx();
        profileItem.Id = profileItem.Id.TrimEx();
        if (profileItem.StreamSecurity.IsNullOrEmpty())
        {
            profileItem.StreamSecurity = Global.StreamSecurity;
        }
        if (profileItem.Id.IsNullOrEmpty())
        {
            return -1;
        }

        await AddServerCommon(config, profileItem, toFile);

        return 0;
    }

    public static async Task<int> AddHysteria2Server(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigType = EConfigType.Hysteria2;
        profileItem.CoreType = ECoreType.sing_box;

        profileItem.Address = profileItem.Address.TrimEx();
        profileItem.Id = profileItem.Id.TrimEx();
        profileItem.Path = profileItem.Path.TrimEx();
        profileItem.Network = string.Empty;

        if (profileItem.StreamSecurity.IsNullOrEmpty())
        {
            profileItem.StreamSecurity = Global.StreamSecurity;
        }
        if (profileItem.Id.IsNullOrEmpty())
        {
            return -1;
        }

        await AddServerCommon(config, profileItem, toFile);

        return 0;
    }

    public static async Task<int> AddTuicServer(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigType = EConfigType.TUIC;
        profileItem.CoreType = ECoreType.sing_box;

        profileItem.Address = profileItem.Address.TrimEx();
        profileItem.Id = profileItem.Id.TrimEx();
        profileItem.Security = profileItem.Security.TrimEx();
        profileItem.Network = string.Empty;

        if (!Global.TuicCongestionControls.Contains(profileItem.HeaderType))
        {
            profileItem.HeaderType = Global.TuicCongestionControls.FirstOrDefault()!;
        }

        if (profileItem.StreamSecurity.IsNullOrEmpty())
        {
            profileItem.StreamSecurity = Global.StreamSecurity;
        }
        if (profileItem.Alpn.IsNullOrEmpty())
        {
            profileItem.Alpn = "h3";
        }
        if (profileItem.Id.IsNullOrEmpty())
        {
            return -1;
        }

        await AddServerCommon(config, profileItem, toFile);

        return 0;
    }

    public static async Task<int> AddWireguardServer(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigType = EConfigType.WireGuard;

        profileItem.Address = profileItem.Address.TrimEx();
        profileItem.Id = profileItem.Id.TrimEx();
        profileItem.PublicKey = profileItem.PublicKey.TrimEx();
        profileItem.Path = profileItem.Path.TrimEx();
        profileItem.RequestHost = profileItem.RequestHost.TrimEx();
        profileItem.Network = string.Empty;
        if (profileItem.ShortId.IsNullOrEmpty())
        {
            profileItem.ShortId = Global.TunMtus.First().ToString();
        }

        if (profileItem.Id.IsNullOrEmpty())
        {
            return -1;
        }

        await AddServerCommon(config, profileItem, toFile);

        return 0;
    }

    public static async Task<int> AddAnytlsServer(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigType = EConfigType.Anytls;
        profileItem.CoreType = ECoreType.sing_box;

        profileItem.Address = profileItem.Address.TrimEx();
        profileItem.Id = profileItem.Id.TrimEx();
        profileItem.Security = profileItem.Security.TrimEx();
        profileItem.Network = string.Empty;
        if (profileItem.StreamSecurity.IsNullOrEmpty())
        {
            profileItem.StreamSecurity = Global.StreamSecurity;
        }
        if (profileItem.Id.IsNullOrEmpty())
        {
            return -1;
        }
        await AddServerCommon(config, profileItem, toFile);
        return 0;
    }

    public static async Task<int> AddVlessServer(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigType = EConfigType.VLESS;

        profileItem.Address = profileItem.Address.TrimEx();
        profileItem.Id = profileItem.Id.TrimEx();
        profileItem.Security = profileItem.Security.TrimEx();
        profileItem.Network = profileItem.Network.TrimEx();
        profileItem.HeaderType = profileItem.HeaderType.TrimEx();
        profileItem.RequestHost = profileItem.RequestHost.TrimEx();
        profileItem.Path = profileItem.Path.TrimEx();
        profileItem.StreamSecurity = profileItem.StreamSecurity.TrimEx();

        if (!Global.Flows.Contains(profileItem.Flow))
        {
            profileItem.Flow = Global.Flows.First();
        }
        if (profileItem.Id.IsNullOrEmpty())
        {
            return -1;
        }
        if (profileItem.Security.IsNullOrEmpty())
        {
            profileItem.Security = Global.None;
        }

        await AddServerCommon(config, profileItem, toFile);

        return 0;
    }

    public static async Task<int> AddCustomServer(Config config, ProfileItem profileItem, bool blDelete)
    {
        var fileName = profileItem.Address;
        if (!File.Exists(fileName))
        {
            return -1;
        }
        var ext = Path.GetExtension(fileName);
        var newFileName = $"{Utils.GetGuid()}{ext}";

        try
        {
            File.Copy(fileName, Utils.GetConfigPath(newFileName));
            if (blDelete)
            {
                File.Delete(fileName);
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
            return -1;
        }

        profileItem.Address = newFileName;
        profileItem.ConfigType = EConfigType.Custom;
        if (profileItem.Remarks.IsNullOrEmpty())
        {
            profileItem.Remarks = $"import custom@{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}";
        }

        await AddServerCommon(config, profileItem, true);

        return 0;
    }

    public static async Task<int> EditCustomServer(Config config, ProfileItem profileItem)
    {
        var item = await AppManager.Instance.GetProfileItem(profileItem.IndexId);
        if (item is null)
        {
            item = profileItem;
        }
        else
        {
            item.Remarks = profileItem.Remarks;
            item.Address = profileItem.Address;
            item.CoreType = profileItem.CoreType;
            item.DisplayLog = profileItem.DisplayLog;
            item.PreSocksPort = profileItem.PreSocksPort;
        }

        if (await SQLiteHelper.Instance.UpdateAsync(item) > 0)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public static async Task<int> RemoveServers(Config config, List<ProfileItem> indexes)
    {
        var subid = "TempRemoveSubId";
        foreach (var item in indexes)
        {
            item.Subid = subid;
        }

        await SQLiteHelper.Instance.UpdateAllAsync(indexes);
        await RemoveServersViaSubid(config, subid, false);

        return 0;
    }

    public static async Task<int> CopyServer(Config config, List<ProfileItem> indexes)
    {
        foreach (var it in indexes)
        {
            var item = await AppManager.Instance.GetProfileItem(it.IndexId);
            if (item is null)
            {
                continue;
            }

            var profileItem = JsonUtils.DeepCopy(item);
            profileItem.IndexId = string.Empty;
            profileItem.Remarks = $"{item.Remarks}-clone";

            if (profileItem.ConfigType == EConfigType.Custom)
            {
                profileItem.Address = Utils.GetConfigPath(profileItem.Address);
                if (await AddCustomServer(config, profileItem, false) == 0)
                {
                }
            }
            else if (profileItem.ConfigType.IsGroupType())
            {
                var profileGroupItem = await AppManager.Instance.GetProfileGroupItem(it.IndexId);
                await AddGroupServerCommon(config, profileItem, profileGroupItem, true);
            }
            else
            {
                await AddServerCommon(config, profileItem, true);
            }
        }

        return 0;
    }

    public static async Task<int> SetDefaultServerIndex(Config config, string? indexId)
    {
        if (indexId.IsNullOrEmpty())
        {
            return -1;
        }

        config.IndexId = indexId;

        await SaveConfig(config);

        return 0;
    }

    public static async Task<int> SetDefaultServer(Config config, List<ProfileItemModel> lstProfile)
    {
        if (lstProfile.Exists(t => t.IndexId == config.IndexId))
        {
            return 0;
        }

        if (await SQLiteHelper.Instance.TableAsync<ProfileItem>().FirstOrDefaultAsync(t => t.IndexId == config.IndexId) != null)
        {
            return 0;
        }
        if (lstProfile.Count > 0)
        {
            return await SetDefaultServerIndex(config, lstProfile.FirstOrDefault(t => t.Port > 0)?.IndexId);
        }

        var item = await SQLiteHelper.Instance.TableAsync<ProfileItem>().FirstOrDefaultAsync(t => t.Port > 0);
        return await SetDefaultServerIndex(config, item?.IndexId);
    }

    public static async Task<ProfileItem?> GetDefaultServer(Config config)
    {
        var item = await AppManager.Instance.GetProfileItem(config.IndexId);
        if (item is null)
        {
            var item2 = await SQLiteHelper.Instance.TableAsync<ProfileItem>().FirstOrDefaultAsync();
            await SetDefaultServerIndex(config, item2?.IndexId);
            return item2;
        }

        return item;
    }

    public static async Task<int> MoveServer(Config config, List<ProfileItem> lstProfile, int index, EMove eMove, int pos = -1)
    {
        var count = lstProfile.Count;
        if (index < 0 || index > lstProfile.Count - 1)
        {
            return -1;
        }

        for (var i = 0; i < lstProfile.Count; i++)
        {
            ProfileExManager.Instance.SetSort(lstProfile[i].IndexId, (i + 1) * 10);
        }

        var sort = 0;
        switch (eMove)
        {
            case EMove.Top:
                {
                    if (index == 0)
                    {
                        return 0;
                    }
                    sort = ProfileExManager.Instance.GetSort(lstProfile.First().IndexId) - 1;

                    break;
                }
            case EMove.Up:
                {
                    if (index == 0)
                    {
                        return 0;
                    }
                    sort = ProfileExManager.Instance.GetSort(lstProfile[index - 1].IndexId) - 1;

                    break;
                }

            case EMove.Down:
                {
                    if (index == count - 1)
                    {
                        return 0;
                    }
                    sort = ProfileExManager.Instance.GetSort(lstProfile[index + 1].IndexId) + 1;

                    break;
                }
            case EMove.Bottom:
                {
                    if (index == count - 1)
                    {
                        return 0;
                    }
                    sort = ProfileExManager.Instance.GetSort(lstProfile[^1].IndexId) + 1;

                    break;
                }
            case EMove.Position:
                sort = (pos * 10) + 1;
                break;
        }

        ProfileExManager.Instance.SetSort(lstProfile[index].IndexId, sort);
        return await Task.FromResult(0);
    }

    public static async Task<int> SortServers(Config config, string subId, string colName, bool asc)
    {
        var lstModel = await AppManager.Instance.ProfileItems(subId, "");
        if (lstModel.Count <= 0)
        {
            return -1;
        }
        var lstServerStat = (config.GuiItem.EnableStatistics ? StatisticsManager.Instance.ServerStat : null) ?? [];
        var lstProfileExs = await ProfileExManager.Instance.GetProfileExs();
        var lstProfile = (from t in lstModel
                          join t2 in lstServerStat on t.IndexId equals t2.IndexId into t2b
                          from t22 in t2b.DefaultIfEmpty()
                          join t3 in lstProfileExs on t.IndexId equals t3.IndexId into t3b
                          from t33 in t3b.DefaultIfEmpty()
                          select new ProfileItemModel
                          {
                              IndexId = t.IndexId,
                              ConfigType = t.ConfigType,
                              Remarks = t.Remarks,
                              Address = t.Address,
                              Port = t.Port,
                              Security = t.Security,
                              Network = t.Network,
                              StreamSecurity = t.StreamSecurity,
                              Delay = t33?.Delay ?? 0,
                              Speed = t33?.Speed ?? 0,
                              Sort = t33?.Sort ?? 0,
                              TodayDown = (t22?.TodayDown ?? 0).ToString("D16"),
                              TodayUp = (t22?.TodayUp ?? 0).ToString("D16"),
                              TotalDown = (t22?.TotalDown ?? 0).ToString("D16"),
                              TotalUp = (t22?.TotalUp ?? 0).ToString("D16"),
                          }).ToList();

        Enum.TryParse(colName, true, out EServerColName name);

        if (asc)
        {
            lstProfile = name switch
            {
                EServerColName.ConfigType => lstProfile.OrderBy(t => t.ConfigType).ToList(),
                EServerColName.Remarks => lstProfile.OrderBy(t => t.Remarks).ToList(),
                EServerColName.Address => lstProfile.OrderBy(t => t.Address).ToList(),
                EServerColName.Port => lstProfile.OrderBy(t => t.Port).ToList(),
                EServerColName.Network => lstProfile.OrderBy(t => t.Network).ToList(),
                EServerColName.StreamSecurity => lstProfile.OrderBy(t => t.StreamSecurity).ToList(),
                EServerColName.DelayVal => lstProfile.OrderBy(t => t.Delay).ToList(),
                EServerColName.SpeedVal => lstProfile.OrderBy(t => t.Speed).ToList(),
                EServerColName.SubRemarks => lstProfile.OrderBy(t => t.Subid).ToList(),
                EServerColName.TodayDown => lstProfile.OrderBy(t => t.TodayDown).ToList(),
                EServerColName.TodayUp => lstProfile.OrderBy(t => t.TodayUp).ToList(),
                EServerColName.TotalDown => lstProfile.OrderBy(t => t.TotalDown).ToList(),
                EServerColName.TotalUp => lstProfile.OrderBy(t => t.TotalUp).ToList(),
                _ => lstProfile
            };
        }
        else
        {
            lstProfile = name switch
            {
                EServerColName.ConfigType => lstProfile.OrderByDescending(t => t.ConfigType).ToList(),
                EServerColName.Remarks => lstProfile.OrderByDescending(t => t.Remarks).ToList(),
                EServerColName.Address => lstProfile.OrderByDescending(t => t.Address).ToList(),
                EServerColName.Port => lstProfile.OrderByDescending(t => t.Port).ToList(),
                EServerColName.Network => lstProfile.OrderByDescending(t => t.Network).ToList(),
                EServerColName.StreamSecurity => lstProfile.OrderByDescending(t => t.StreamSecurity).ToList(),
                EServerColName.DelayVal => lstProfile.OrderByDescending(t => t.Delay).ToList(),
                EServerColName.SpeedVal => lstProfile.OrderByDescending(t => t.Speed).ToList(),
                EServerColName.SubRemarks => lstProfile.OrderByDescending(t => t.Subid).ToList(),
                EServerColName.TodayDown => lstProfile.OrderByDescending(t => t.TodayDown).ToList(),
                EServerColName.TodayUp => lstProfile.OrderByDescending(t => t.TodayUp).ToList(),
                EServerColName.TotalDown => lstProfile.OrderByDescending(t => t.TotalDown).ToList(),
                EServerColName.TotalUp => lstProfile.OrderByDescending(t => t.TotalUp).ToList(),
                _ => lstProfile
            };
        }

        for (var i = 0; i < lstProfile.Count; i++)
        {
            ProfileExManager.Instance.SetSort(lstProfile[i].IndexId, (i + 1) * 10);
        }
        switch (name)
        {
            case EServerColName.DelayVal:
                {
                    var maxSort = lstProfile.Max(t => t.Sort) + 10;
                    foreach (var item in lstProfile.Where(item => item.Delay <= 0))
                    {
                        ProfileExManager.Instance.SetSort(item.IndexId, maxSort);
                    }

                    break;
                }
            case EServerColName.SpeedVal:
                {
                    var maxSort = lstProfile.Max(t => t.Sort) + 10;
                    foreach (var item in lstProfile.Where(item => item.Speed <= 0))
                    {
                        ProfileExManager.Instance.SetSort(item.IndexId, maxSort);
                    }

                    break;
                }
        }

        return 0;
    }

    public static async Task<Tuple<int, int>> DedupServerList(Config config, string subId)
    {
        var lstProfile = await AppManager.Instance.ProfileItems(subId);
        if (lstProfile == null)
        {
            return new Tuple<int, int>(0, 0);
        }

        List<ProfileItem> lstKeep = new();
        List<ProfileItem> lstRemove = new();
        if (!config.GuiItem.KeepOlderDedupl)
        {
            lstProfile.Reverse();
        }

        foreach (var item in lstProfile)
        {
            if (!lstKeep.Exists(i => CompareProfileItem(i, item, false)))
            {
                lstKeep.Add(item);
            }
            else
            {
                lstRemove.Add(item);
            }
        }
        await RemoveServers(config, lstRemove);

        return new Tuple<int, int>(lstProfile.Count, lstKeep.Count);
    }

    public static async Task<int> AddServerCommon(Config config, ProfileItem profileItem, bool toFile = true)
    {
        profileItem.ConfigVersion = 2;

        if (profileItem.StreamSecurity.IsNotEmpty())
        {
            if (profileItem.StreamSecurity != Global.StreamSecurity
                 && profileItem.StreamSecurity != Global.StreamSecurityReality)
            {
                profileItem.StreamSecurity = string.Empty;
            }
            else
            {
                if (profileItem.AllowInsecure.IsNullOrEmpty())
                {
                    profileItem.AllowInsecure = config.CoreBasicItem.DefAllowInsecure.ToString().ToLower();
                }
                if (profileItem.Fingerprint.IsNullOrEmpty() && profileItem.StreamSecurity == Global.StreamSecurityReality)
                {
                    profileItem.Fingerprint = config.CoreBasicItem.DefFingerprint;
                }
            }
        }

        if (profileItem.Network.IsNotEmpty() && !Global.Networks.Contains(profileItem.Network))
        {
            profileItem.Network = Global.DefaultNetwork;
        }

        var maxSort = -1;
        if (profileItem.IndexId.IsNullOrEmpty())
        {
            profileItem.IndexId = Utils.GetGuid(false);
            maxSort = ProfileExManager.Instance.GetMaxSort();
        }
        if (!toFile && maxSort < 0)
        {
            maxSort = ProfileExManager.Instance.GetMaxSort();
        }
        if (maxSort > 0)
        {
            ProfileExManager.Instance.SetSort(profileItem.IndexId, maxSort + 1);
        }

        if (toFile)
        {
            await SQLiteHelper.Instance.ReplaceAsync(profileItem);
        }
        return 0;
    }

    public static async Task<int> AddGroupServerCommon(Config config, ProfileItem profileItem, ProfileGroupItem profileGroupItem, bool toFile = true)
    {
        var maxSort = -1;
        if (profileItem.IndexId.IsNullOrEmpty())
        {
            profileItem.IndexId = Utils.GetGuid(false);
            maxSort = ProfileExManager.Instance.GetMaxSort();
        }
        var groupType = profileItem.ConfigType == EConfigType.ProxyChain ? EConfigType.ProxyChain.ToString() : profileGroupItem.MultipleLoad.ToString();
        profileItem.Address = $"{profileItem.CoreType}-{groupType}";
        if (maxSort > 0)
        {
            ProfileExManager.Instance.SetSort(profileItem.IndexId, maxSort + 1);
        }
        if (toFile)
        {
            await SQLiteHelper.Instance.ReplaceAsync(profileItem);
            if (profileGroupItem != null)
            {
                profileGroupItem.IndexId = profileItem.IndexId;
                await ProfileGroupItemManager.Instance.SaveItemAsync(profileGroupItem);
            }
            else
            {
                ProfileGroupItemManager.Instance.GetOrCreateAndMarkDirty(profileItem.IndexId);
                await ProfileGroupItemManager.Instance.SaveTo();
            }
        }
        return 0;
    }

    private static bool CompareProfileItem(ProfileItem? o, ProfileItem? n, bool remarks)
    {
        if (o == null || n == null)
        {
            return false;
        }

        return o.ConfigType == n.ConfigType
               && AreEqual(o.Address, n.Address)
               && o.Port == n.Port
               && AreEqual(o.Id, n.Id)
               && AreEqual(o.Security, n.Security)
               && AreEqual(o.Network, n.Network)
               && AreEqual(o.HeaderType, n.HeaderType)
               && AreEqual(o.RequestHost, n.RequestHost)
               && AreEqual(o.Path, n.Path)
               && (o.ConfigType == EConfigType.Trojan || o.StreamSecurity == n.StreamSecurity)
               && AreEqual(o.Flow, n.Flow)
               && AreEqual(o.Sni, n.Sni)
               && AreEqual(o.Alpn, n.Alpn)
               && AreEqual(o.Fingerprint, n.Fingerprint)
               && AreEqual(o.PublicKey, n.PublicKey)
               && AreEqual(o.ShortId, n.ShortId)
               && (!remarks || o.Remarks == n.Remarks);

        static bool AreEqual(string? a, string? b)
        {
            return string.Equals(a, b) || (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b));
        }
    }

    private static async Task<int> RemoveProfileItem(Config config, string indexId)
    {
        try
        {
            var item = await AppManager.Instance.GetProfileItem(indexId);
            if (item == null)
            {
                return 0;
            }
            if (item.ConfigType == EConfigType.Custom)
            {
                File.Delete(Utils.GetConfigPath(item.Address));
            }

            await SQLiteHelper.Instance.DeleteAsync(item);
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }

        return 0;
    }

    public static async Task<RetResult> AddGroupServer4Multiple(Config config, List<ProfileItem> selecteds, ECoreType coreType, EMultipleLoad multipleLoad, string? subId)
    {
        var result = new RetResult();

        var indexId = Utils.GetGuid(false);
        var childProfileIndexId = Utils.List2String(selecteds.Select(p => p.IndexId).ToList());

        var remark = subId.IsNullOrEmpty() ? string.Empty : $"{(await AppManager.Instance.GetSubItem(subId)).Remarks} ";
        if (coreType == ECoreType.Xray)
        {
            remark += multipleLoad switch
            {
                EMultipleLoad.LeastPing => ResUI.menuGenGroupMultipleServerXrayLeastPing,
                EMultipleLoad.Fallback => ResUI.menuGenGroupMultipleServerXrayFallback,
                EMultipleLoad.Random => ResUI.menuGenGroupMultipleServerXrayRandom,
                EMultipleLoad.RoundRobin => ResUI.menuGenGroupMultipleServerXrayRoundRobin,
                EMultipleLoad.LeastLoad => ResUI.menuGenGroupMultipleServerXrayLeastLoad,
                _ => ResUI.menuGenGroupMultipleServerXrayRoundRobin,
            };
        }
        else if (coreType == ECoreType.sing_box)
        {
            remark += multipleLoad switch
            {
                EMultipleLoad.LeastPing => ResUI.menuGenGroupMultipleServerSingBoxLeastPing,
                EMultipleLoad.Fallback => ResUI.menuGenGroupMultipleServerSingBoxFallback,
                _ => ResUI.menuGenGroupMultipleServerSingBoxLeastPing,
            };
        }
        var profile = new ProfileItem
        {
            IndexId = indexId,
            CoreType = coreType,
            ConfigType = EConfigType.PolicyGroup,
            Remarks = remark,
            IsSub = false
        };
        if (!subId.IsNullOrEmpty())
        {
            profile.Subid = subId;
        }
        var profileGroup = new ProfileGroupItem
        {
            ChildItems = childProfileIndexId,
            MultipleLoad = multipleLoad,
            IndexId = indexId,
        };
        var ret = await AddGroupServerCommon(config, profile, profileGroup, true);
        result.Success = ret == 0;
        result.Data = indexId;
        return result;
    }

    public static async Task<ProfileItem?> GetPreSocksItem(Config config, ProfileItem node, ECoreType coreType)
    {
        ProfileItem? itemSocks = null;
        if (node.ConfigType != EConfigType.Custom && coreType != ECoreType.sing_box && config.TunModeItem.EnableTun)
        {
            var tun2SocksAddress = node.Address;
            if (node.ConfigType.IsGroupType())
            {
                var lstAddresses = (await ProfileGroupItemManager.GetAllChildDomainAddresses(node.IndexId)).ToList();
                if (lstAddresses.Count > 0)
                {
                    tun2SocksAddress = Utils.List2String(lstAddresses);
                }
            }
            itemSocks = new ProfileItem()
            {
                CoreType = ECoreType.sing_box,
                ConfigType = EConfigType.SOCKS,
                Address = Global.Loopback,
                SpiderX = tun2SocksAddress,
                Port = AppManager.Instance.GetLocalPort(EInboundProtocol.socks)
            };
        }
        else if (node.ConfigType == EConfigType.Custom && node.PreSocksPort > 0)
        {
            var preCoreType = config.RunningCoreType = config.TunModeItem.EnableTun ? ECoreType.sing_box : ECoreType.Xray;
            itemSocks = new ProfileItem()
            {
                CoreType = preCoreType,
                ConfigType = EConfigType.SOCKS,
                Address = Global.Loopback,
                Port = node.PreSocksPort.Value,
            };
        }
        await Task.CompletedTask;
        return itemSocks;
    }

    public static async Task<int> RemoveInvalidServerResult(Config config, string subid)
    {
        var lstModel = await AppManager.Instance.ProfileItems(subid, "");
        if (lstModel is { Count: <= 0 })
        {
            return -1;
        }
        var lstProfileExs = await ProfileExManager.Instance.GetProfileExs();
        var lstProfile = (from t in lstModel
                          join t2 in lstProfileExs on t.IndexId equals t2.IndexId
                          where t2.Delay == -1
                          select t).ToList();

        await RemoveServers(config, JsonUtils.Deserialize<List<ProfileItem>>(JsonUtils.Serialize(lstProfile)));

        return lstProfile.Count;
    }

    #endregion Server
}
