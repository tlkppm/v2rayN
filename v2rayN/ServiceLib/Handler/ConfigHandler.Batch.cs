using System.Text.RegularExpressions;

namespace ServiceLib.Handler;

public static partial class ConfigHandler
{
    #region Batch add servers

    private static async Task<int> AddBatchServersCommon(Config config, string strData, string subid, bool isSub)
    {
        if (strData.IsNullOrEmpty())
        {
            return -1;
        }

        var subFilter = string.Empty;
        if (isSub && subid.IsNotEmpty())
        {
            await RemoveServersViaSubid(config, subid, isSub);
            subFilter = (await AppManager.Instance.GetSubItem(subid))?.Filter ?? "";
        }

        var countServers = 0;
        List<ProfileItem> lstAdd = new();
        var arrData = strData.Split(Environment.NewLine.ToCharArray()).Where(t => !t.IsNullOrEmpty());
        if (isSub)
        {
            arrData = arrData.Distinct();
        }
        foreach (var str in arrData)
        {
            if (!isSub && (str.StartsWith(Global.HttpsProtocol) || str.StartsWith(Global.HttpProtocol)))
            {
                if (await AddSubItem(config, str) == 0)
                {
                    countServers++;
                }
                continue;
            }
            var profileItem = FmtHandler.ResolveConfig(str, out var msg);
            if (profileItem is null)
            {
                continue;
            }

            if (isSub && subid.IsNotEmpty() && subFilter.IsNotEmpty())
            {
                if (!Regex.IsMatch(profileItem.Remarks, subFilter))
                {
                    continue;
                }
            }
            profileItem.Subid = subid;
            profileItem.IsSub = isSub;

            var addStatus = profileItem.ConfigType switch
            {
                EConfigType.VMess => await AddVMessServer(config, profileItem, false),
                EConfigType.Shadowsocks => await AddShadowsocksServer(config, profileItem, false),
                EConfigType.SOCKS => await AddSocksServer(config, profileItem, false),
                EConfigType.Trojan => await AddTrojanServer(config, profileItem, false),
                EConfigType.VLESS => await AddVlessServer(config, profileItem, false),
                EConfigType.Hysteria2 => await AddHysteria2Server(config, profileItem, false),
                EConfigType.TUIC => await AddTuicServer(config, profileItem, false),
                EConfigType.WireGuard => await AddWireguardServer(config, profileItem, false),
                EConfigType.Anytls => await AddAnytlsServer(config, profileItem, false),
                _ => -1,
            };

            if (addStatus == 0)
            {
                countServers++;
                lstAdd.Add(profileItem);
            }
        }

        if (lstAdd.Count > 0)
        {
            await SQLiteHelper.Instance.InsertAllAsync(lstAdd);
        }

        await SaveConfig(config);
        return countServers;
    }

    private static async Task<int> AddBatchServers4Custom(Config config, string strData, string subid, bool isSub)
    {
        if (strData.IsNullOrEmpty())
        {
            return -1;
        }

        var subItem = await AppManager.Instance.GetSubItem(subid);
        var subRemarks = subItem?.Remarks;
        var preSocksPort = subItem?.PreSocksPort;

        List<ProfileItem>? lstProfiles = null;
        if (lstProfiles is null || lstProfiles.Count <= 0)
        {
            lstProfiles = SingboxFmt.ResolveFullArray(strData, subRemarks);
        }
        if (lstProfiles is null || lstProfiles.Count <= 0)
        {
            lstProfiles = V2rayFmt.ResolveFullArray(strData, subRemarks);
        }
        if (lstProfiles != null && lstProfiles.Count > 0)
        {
            if (isSub && subid.IsNotEmpty())
            {
                await RemoveServersViaSubid(config, subid, isSub);
            }
            var count = 0;
            foreach (var it in lstProfiles)
            {
                it.Subid = subid;
                it.IsSub = isSub;
                it.PreSocksPort = preSocksPort;
                if (await AddCustomServer(config, it, true) == 0)
                {
                    count++;
                }
            }
            if (count > 0)
            {
                return count;
            }
        }

        ProfileItem? profileItem = null;
        if (profileItem is null)
        {
            profileItem = SingboxFmt.ResolveFull(strData, subRemarks);
        }
        if (profileItem is null)
        {
            profileItem = V2rayFmt.ResolveFull(strData, subRemarks);
        }
        if (profileItem is null && HtmlPageFmt.IsHtmlPage(strData))
        {
            return -1;
        }
        if (profileItem is null)
        {
            profileItem = ClashFmt.ResolveFull(strData, subRemarks);
        }
        if (profileItem is null)
        {
            profileItem = Hysteria2Fmt.ResolveFull2(strData, subRemarks);
        }
        if (profileItem is null || profileItem.Address.IsNullOrEmpty())
        {
            return -1;
        }

        if (isSub && subid.IsNotEmpty())
        {
            await RemoveServersViaSubid(config, subid, isSub);
        }

        profileItem.Subid = subid;
        profileItem.IsSub = isSub;
        profileItem.PreSocksPort = preSocksPort;
        if (await AddCustomServer(config, profileItem, true) == 0)
        {
            return 1;
        }
        return -1;
    }

    private static async Task<int> AddBatchServers4SsSIP008(Config config, string strData, string subid, bool isSub)
    {
        if (strData.IsNullOrEmpty())
        {
            return -1;
        }

        if (isSub && subid.IsNotEmpty())
        {
            await RemoveServersViaSubid(config, subid, isSub);
        }

        var lstSsServer = ShadowsocksFmt.ResolveSip008(strData);
        if (lstSsServer?.Count > 0)
        {
            var counter = 0;
            foreach (var ssItem in lstSsServer)
            {
                ssItem.Subid = subid;
                ssItem.IsSub = isSub;
                if (await AddShadowsocksServer(config, ssItem) == 0)
                {
                    counter++;
                }
            }
            await SaveConfig(config);
            return counter;
        }

        return -1;
    }

    public static async Task<int> AddBatchServers(Config config, string strData, string subid, bool isSub)
    {
        if (strData.IsNullOrEmpty())
        {
            return -1;
        }
        List<ProfileItem>? lstOriSub = null;
        ProfileItem? activeProfile = null;
        if (isSub && subid.IsNotEmpty())
        {
            lstOriSub = await AppManager.Instance.ProfileItems(subid);
            activeProfile = lstOriSub?.FirstOrDefault(t => t.IndexId == config.IndexId);
        }

        var counter = 0;
        if (Utils.IsBase64String(strData))
        {
            counter = await AddBatchServersCommon(config, Utils.Base64Decode(strData), subid, isSub);
        }
        if (counter < 1)
        {
            counter = await AddBatchServersCommon(config, strData, subid, isSub);
        }
        if (counter < 1)
        {
            counter = await AddBatchServersCommon(config, Utils.Base64Decode(strData), subid, isSub);
        }

        if (counter < 1)
        {
            counter = await AddBatchServers4SsSIP008(config, strData, subid, isSub);
        }

        if (counter < 1)
        {
            counter = await AddBatchServers4Custom(config, strData, subid, isSub);
        }

        if (activeProfile != null)
        {
            var lstSub = await AppManager.Instance.ProfileItems(subid);
            var existItem = lstSub?.FirstOrDefault(t => config.UiItem.EnableUpdateSubOnlyRemarksExist ? t.Remarks == activeProfile.Remarks : CompareProfileItem(t, activeProfile, true));
            if (existItem != null)
            {
                await ConfigHandler.SetDefaultServerIndex(config, existItem.IndexId);
            }
        }

        if (lstOriSub != null)
        {
            var lstSub = await AppManager.Instance.ProfileItems(subid);
            foreach (var item in lstSub)
            {
                var existItem = lstOriSub?.FirstOrDefault(t => config.UiItem.EnableUpdateSubOnlyRemarksExist ? t.Remarks == item.Remarks : CompareProfileItem(t, item, true));
                if (existItem != null)
                {
                    await StatisticsManager.Instance.CloneServerStatItem(existItem.IndexId, item.IndexId);
                }
            }
        }

        return counter;
    }

    #endregion Batch add servers
}
