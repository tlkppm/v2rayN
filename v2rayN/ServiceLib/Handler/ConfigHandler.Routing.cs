namespace ServiceLib.Handler;

public static partial class ConfigHandler
{
    #region Routing

    public static async Task<int> SaveRoutingItem(Config config, RoutingItem item)
    {
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

    public static async Task<int> AddBatchRoutingRules(RoutingItem routingItem, string strData)
    {
        if (strData.IsNullOrEmpty())
        {
            return -1;
        }

        var lstRules = JsonUtils.Deserialize<List<RulesItem>>(strData);
        if (lstRules == null)
        {
            return -1;
        }

        foreach (var item in lstRules)
        {
            item.Id = Utils.GetGuid(false);
        }
        routingItem.RuleNum = lstRules.Count;
        routingItem.RuleSet = JsonUtils.Serialize(lstRules, false);

        if (routingItem.Id.IsNullOrEmpty())
        {
            routingItem.Id = Utils.GetGuid(false);
        }

        if (await SQLiteHelper.Instance.ReplaceAsync(routingItem) > 0)
        {
            return 0;
        }
        return -1;
    }

    public static async Task<int> MoveRoutingRule(List<RulesItem> rules, int index, EMove eMove, int pos = -1)
    {
        var count = rules.Count;
        if (index < 0 || index > rules.Count - 1)
        {
            return -1;
        }
        switch (eMove)
        {
            case EMove.Top:
                {
                    if (index == 0) return 0;
                    var item = JsonUtils.DeepCopy(rules[index]);
                    rules.RemoveAt(index);
                    rules.Insert(0, item);
                    break;
                }
            case EMove.Up:
                {
                    if (index == 0) return 0;
                    var item = JsonUtils.DeepCopy(rules[index]);
                    rules.RemoveAt(index);
                    rules.Insert(index - 1, item);
                    break;
                }
            case EMove.Down:
                {
                    if (index == count - 1) return 0;
                    var item = JsonUtils.DeepCopy(rules[index]);
                    rules.RemoveAt(index);
                    rules.Insert(index + 1, item);
                    break;
                }
            case EMove.Bottom:
                {
                    if (index == count - 1) return 0;
                    var item = JsonUtils.DeepCopy(rules[index]);
                    rules.RemoveAt(index);
                    rules.Add(item);
                    break;
                }
            case EMove.Position:
                {
                    var removeItem = rules[index];
                    var item = JsonUtils.DeepCopy(rules[index]);
                    rules.Insert(pos, item);
                    rules.Remove(removeItem);
                    break;
                }
        }
        return await Task.FromResult(0);
    }

    public static async Task<int> SetDefaultRouting(Config config, RoutingItem routingItem)
    {
        var items = await AppManager.Instance.RoutingItems();
        if (items.Any(t => t.Id == routingItem.Id && t.IsActive == true))
        {
            return -1;
        }

        foreach (var item in items)
        {
            item.IsActive = item.Id == routingItem.Id;
        }

        await SQLiteHelper.Instance.UpdateAllAsync(items);
        return 0;
    }

    public static async Task<RoutingItem> GetDefaultRouting(Config config)
    {
        var item = await SQLiteHelper.Instance.TableAsync<RoutingItem>().FirstOrDefaultAsync(it => it.IsActive == true);
        if (item is null)
        {
            var item2 = await SQLiteHelper.Instance.TableAsync<RoutingItem>().FirstOrDefaultAsync();
            await SetDefaultRouting(config, item2);
            return item2;
        }
        return item;
    }

    public static async Task<int> InitRouting(Config config, bool blImportAdvancedRules = false)
    {
        if (config.ConstItem.RouteRulesTemplateSourceUrl.IsNullOrEmpty())
        {
            await InitBuiltinRouting(config, blImportAdvancedRules);
        }
        else
        {
            await InitExternalRouting(config, blImportAdvancedRules);
        }
        return 0;
    }

    public static async Task<int> InitExternalRouting(Config config, bool blImportAdvancedRules = false)
    {
        var downloadHandle = new DownloadService();
        var templateContent = await downloadHandle.TryDownloadString(config.ConstItem.RouteRulesTemplateSourceUrl, true, "");
        if (templateContent.IsNullOrEmpty())
        {
            return await InitBuiltinRouting(config, blImportAdvancedRules);
        }

        var template = JsonUtils.Deserialize<RoutingTemplate>(templateContent);
        if (template == null)
        {
            return await InitBuiltinRouting(config, blImportAdvancedRules);
        }

        var items = await AppManager.Instance.RoutingItems();
        var maxSort = items.Count;
        if (!blImportAdvancedRules && items.Where(t => t.Remarks.StartsWith(template.Version)).ToList().Count > 0)
        {
            return 0;
        }
        for (var i = 0; i < template.RoutingItems.Length; i++)
        {
            var item = template.RoutingItems[i];

            if (item.Url.IsNullOrEmpty() && item.RuleSet.IsNullOrEmpty())
            {
                continue;
            }

            var ruleSetsString = !item.RuleSet.IsNullOrEmpty()
                ? item.RuleSet
                : await downloadHandle.TryDownloadString(item.Url, true, "");

            if (ruleSetsString.IsNullOrEmpty())
            {
                continue;
            }

            item.Remarks = $"{template.Version}-{item.Remarks}";
            item.Enabled = true;
            item.Sort = ++maxSort;
            item.Url = string.Empty;

            await AddBatchRoutingRules(item, ruleSetsString);

            if (!blImportAdvancedRules && i == 0)
            {
                await SetDefaultRouting(config, item);
            }
        }

        return 0;
    }

    public static async Task<int> InitBuiltinRouting(Config config, bool blImportAdvancedRules = false)
    {
        var ver = "V4-";
        var items = await AppManager.Instance.RoutingItems();

        var lockItem = items?.FirstOrDefault(t => t.Locked == true);
        if (lockItem != null)
        {
            await ConfigHandler.RemoveRoutingItem(lockItem);
            items = await AppManager.Instance.RoutingItems();
        }

        if (!blImportAdvancedRules && items.Count(u => u.Remarks.StartsWith(ver)) > 0)
        {
            if (config.RoutingBasicItem.RoutingIndexId.IsNotEmpty())
            {
                var item = items.FirstOrDefault(t => t.Id == config.RoutingBasicItem.RoutingIndexId);
                if (item != null)
                {
                    await SetDefaultRouting(config, item);
                }
                config.RoutingBasicItem.RoutingIndexId = string.Empty;
            }
            return 0;
        }

        var maxSort = items.Count;

        var item2 = new RoutingItem()
        {
            Remarks = $"{ver}绕过大陆(Whitelist)",
            Url = string.Empty,
            Sort = maxSort + 1,
        };
        await AddBatchRoutingRules(item2, EmbedUtils.GetEmbedText(Global.CustomRoutingFileName + "white"));

        var item3 = new RoutingItem()
        {
            Remarks = $"{ver}黑名单(Blacklist)",
            Url = string.Empty,
            Sort = maxSort + 2,
        };
        await AddBatchRoutingRules(item3, EmbedUtils.GetEmbedText(Global.CustomRoutingFileName + "black"));

        var item1 = new RoutingItem()
        {
            Remarks = $"{ver}全局(Global)",
            Url = string.Empty,
            Sort = maxSort + 3,
        };
        await AddBatchRoutingRules(item1, EmbedUtils.GetEmbedText(Global.CustomRoutingFileName + "global"));

        if (!blImportAdvancedRules)
        {
            await SetDefaultRouting(config, item2);
        }
        return 0;
    }

    public static async Task RemoveRoutingItem(RoutingItem routingItem)
    {
        await SQLiteHelper.Instance.DeleteAsync(routingItem);
    }

    #endregion Routing
}
