using System.Data;

namespace ServiceLib.Handler;

public static partial class ConfigHandler
{
    private static readonly string _configRes = Global.ConfigFileName;
    private static readonly string _tag = "ConfigHandler";

    #region ConfigHandler

    /// <summary>
    /// Load the application configuration file
    /// If the file exists, deserialize it from JSON
    /// If not found, create a new Config object with default settings
    /// Initialize default values for missing configuration sections
    /// </summary>
    /// <returns>Config object containing application settings or null if there's an error</returns>
    public static Config? LoadConfig()
    {
        Config? config = null;
        var result = EmbedUtils.LoadResource(Utils.GetConfigPath(_configRes));
        if (result.IsNotEmpty())
        {
            config = JsonUtils.Deserialize<Config>(result);
        }
        else
        {
            if (File.Exists(Utils.GetConfigPath(_configRes)))
            {
                Logging.SaveLog("LoadConfig Exception");
                return null;
            }
        }

        config ??= new Config();

        config.CoreBasicItem ??= new()
        {
            LogEnabled = false,
            Loglevel = "warning",
            MuxEnabled = false,
        };

        if (config.Inbound == null)
        {
            config.Inbound = new List<InItem>();
            InItem inItem = new()
            {
                Protocol = EInboundProtocol.socks.ToString(),
                LocalPort = 10808,
                UdpEnabled = true,
                SniffingEnabled = true,
                RouteOnly = false,
            };

            config.Inbound.Add(inItem);
        }
        else
        {
            if (config.Inbound.Count > 0)
            {
                config.Inbound.First().Protocol = EInboundProtocol.socks.ToString();
            }
        }

        config.RoutingBasicItem ??= new();
        if (config.RoutingBasicItem.DomainStrategy.IsNullOrEmpty())
        {
            config.RoutingBasicItem.DomainStrategy = Global.DomainStrategies.First();
        }

        config.KcpItem ??= new KcpItem
        {
            Mtu = 1350,
            Tti = 50,
            UplinkCapacity = 12,
            DownlinkCapacity = 100,
            ReadBufferSize = 2,
            WriteBufferSize = 2,
            Congestion = false
        };
        config.GrpcItem ??= new GrpcItem
        {
            IdleTimeout = 60,
            HealthCheckTimeout = 20,
            PermitWithoutStream = false,
            InitialWindowsSize = 0,
        };
        config.TunModeItem ??= new TunModeItem
        {
            EnableTun = false,
            Mtu = 9000,
        };
        config.GuiItem ??= new();
        config.MsgUIItem ??= new();

        config.UiItem ??= new UIItem()
        {
            EnableUpdateSubOnlyRemarksExist = true
        };
        config.UiItem.MainColumnItem ??= new();
        config.UiItem.WindowSizeItem ??= new();

        if (config.UiItem.CurrentLanguage.IsNullOrEmpty())
        {
            config.UiItem.CurrentLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Equals("zh", StringComparison.CurrentCultureIgnoreCase)
                ? Global.Languages.First()
                : Global.Languages[2];
        }

        config.ConstItem ??= new ConstItem();

        config.SimpleDNSItem ??= InitBuiltinSimpleDNS();
        config.SimpleDNSItem.GlobalFakeIp ??= true;
        config.SimpleDNSItem.BootstrapDNS ??= Global.DomainPureIPDNSAddress.FirstOrDefault();

        config.SpeedTestItem ??= new();
        if (config.SpeedTestItem.SpeedTestTimeout < 10)
        {
            config.SpeedTestItem.SpeedTestTimeout = 10;
        }
        if (config.SpeedTestItem.SpeedTestUrl.IsNullOrEmpty())
        {
            config.SpeedTestItem.SpeedTestUrl = Global.SpeedTestUrls.First();
        }
        if (config.SpeedTestItem.SpeedPingTestUrl.IsNullOrEmpty())
        {
            config.SpeedTestItem.SpeedPingTestUrl = Global.SpeedPingTestUrls.First();
        }
        if (config.SpeedTestItem.MixedConcurrencyCount < 1)
        {
            config.SpeedTestItem.MixedConcurrencyCount = 5;
        }

        config.Mux4RayItem ??= new()
        {
            Concurrency = 8,
            XudpConcurrency = 16,
            XudpProxyUDP443 = "reject"
        };

        config.Mux4SboxItem ??= new()
        {
            Protocol = Global.SingboxMuxs.First(),
            MaxConnections = 8
        };

        config.HysteriaItem ??= new()
        {
            UpMbps = 100,
            DownMbps = 100
        };
        config.ClashUIItem ??= new();
        config.SystemProxyItem ??= new();
        config.WebDavItem ??= new();
        config.CheckUpdateItem ??= new();
        config.Fragment4RayItem ??= new()
        {
            Packets = "tlshello",
            Length = "100-200",
            Interval = "10-20"
        };
        config.GlobalHotkeys ??= new();

        if (config.SystemProxyItem.SystemProxyExceptions.IsNullOrEmpty())
        {
            config.SystemProxyItem.SystemProxyExceptions = Utils.IsWindows() ? Global.SystemProxyExceptionsWindows : Global.SystemProxyExceptionsLinux;
        }

        return config;
    }

    /// <summary>
    /// Save the configuration to a file
    /// First writes to a temporary file, then replaces the original file
    /// </summary>
    /// <param name="config">Configuration object to be saved</param>
    /// <returns>0 if successful, -1 if failed</returns>
    public static async Task<int> SaveConfig(Config config)
    {
        try
        {
            //save temp file
            var resPath = Utils.GetConfigPath(_configRes);
            var tempPath = $"{resPath}_temp";

            var content = JsonUtils.Serialize(config, true, true);
            if (content.IsNullOrEmpty())
            {
                return -1;
            }
            await File.WriteAllTextAsync(tempPath, content);

            //rename
            File.Move(tempPath, resPath, true);
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
            return -1;
        }

        return 0;
    }

    #endregion ConfigHandler


    #region Custom Config

    public static async Task<int> InitBuiltinFullConfigTemplate(Config config)
    {
        var items = await AppManager.Instance.FullConfigTemplateItem();
        if (items.Count <= 0)
        {
            var item = new FullConfigTemplateItem()
            {
                Remarks = "V2ray",
                CoreType = ECoreType.Xray,
            };
            await SaveFullConfigTemplate(config, item);

            var item2 = new FullConfigTemplateItem()
            {
                Remarks = "sing-box",
                CoreType = ECoreType.sing_box,
            };
            await SaveFullConfigTemplate(config, item2);
        }

        return 0;
    }

    public static async Task<int> SaveFullConfigTemplate(Config config, FullConfigTemplateItem item)
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
        else
        {
            return -1;
        }
    }

    #endregion Custom Config

    #region Regional Presets

    /// <summary>
    /// Apply regional presets for geo-specific configurations
    /// Sets up geo files, routing rules, and DNS for specific regions
    /// </summary>
    /// <param name="config">Current configuration</param>
    /// <param name="type">Type of preset (Default, Russia, Iran)</param>
    /// <returns>True if successful</returns>
    public static async Task<bool> ApplyRegionalPreset(Config config, EPresetType type)
    {
        switch (type)
        {
            case EPresetType.Default:
                config.ConstItem.GeoSourceUrl = "";
                config.ConstItem.SrsSourceUrl = "";
                config.ConstItem.RouteRulesTemplateSourceUrl = "";

                await SQLiteHelper.Instance.DeleteAllAsync<DNSItem>();
                await InitBuiltinDNS(config);

                config.SimpleDNSItem = InitBuiltinSimpleDNS();
                break;

            case EPresetType.Russia:
                config.ConstItem.GeoSourceUrl = Global.GeoFilesSources[1];
                config.ConstItem.SrsSourceUrl = Global.SingboxRulesetSources[1];
                config.ConstItem.RouteRulesTemplateSourceUrl = Global.RoutingRulesSources[1];

                var xrayDnsRussia = await GetExternalDNSItem(ECoreType.Xray, Global.DNSTemplateSources[1] + "v2ray.json");
                var singboxDnsRussia = await GetExternalDNSItem(ECoreType.sing_box, Global.DNSTemplateSources[1] + "sing_box.json");
                var simpleDnsRussia = await GetExternalSimpleDNSItem(Global.DNSTemplateSources[1] + "simple_dns.json");

                if (simpleDnsRussia == null)
                {
                    xrayDnsRussia.Enabled = true;
                    singboxDnsRussia.Enabled = true;
                    config.SimpleDNSItem = InitBuiltinSimpleDNS();
                }
                else
                {
                    config.SimpleDNSItem = simpleDnsRussia;
                }
                await SaveDNSItems(config, xrayDnsRussia);
                await SaveDNSItems(config, singboxDnsRussia);
                break;

            case EPresetType.Iran:
                config.ConstItem.GeoSourceUrl = Global.GeoFilesSources[2];
                config.ConstItem.SrsSourceUrl = Global.SingboxRulesetSources[2];
                config.ConstItem.RouteRulesTemplateSourceUrl = Global.RoutingRulesSources[2];

                var xrayDnsIran = await GetExternalDNSItem(ECoreType.Xray, Global.DNSTemplateSources[2] + "v2ray.json");
                var singboxDnsIran = await GetExternalDNSItem(ECoreType.sing_box, Global.DNSTemplateSources[2] + "sing_box.json");
                var simpleDnsIran = await GetExternalSimpleDNSItem(Global.DNSTemplateSources[2] + "simple_dns.json");

                if (simpleDnsIran == null)
                {
                    xrayDnsIran.Enabled = true;
                    singboxDnsIran.Enabled = true;
                    config.SimpleDNSItem = InitBuiltinSimpleDNS();
                }
                else
                {
                    config.SimpleDNSItem = simpleDnsIran;
                }
                await SaveDNSItems(config, xrayDnsIran);
                await SaveDNSItems(config, singboxDnsIran);
                break;
        }

        return true;
    }

    #endregion Regional Presets

    #region UIItem

    public static WindowSizeItem? GetWindowSizeItem(Config config, string typeName)
    {
        var sizeItem = config?.UiItem?.WindowSizeItem?.FirstOrDefault(t => t.TypeName == typeName);
        if (sizeItem == null || sizeItem.Width <= 0 || sizeItem.Height <= 0)
        {
            return null;
        }

        return sizeItem;
    }

    public static int SaveWindowSizeItem(Config config, string typeName, double width, double height)
    {
        var sizeItem = config?.UiItem?.WindowSizeItem?.FirstOrDefault(t => t.TypeName == typeName);
        if (sizeItem == null)
        {
            sizeItem = new WindowSizeItem { TypeName = typeName };
            config.UiItem.WindowSizeItem.Add(sizeItem);
        }

        sizeItem.Width = (int)width;
        sizeItem.Height = (int)height;

        return 0;
    }

    public static int SaveMainGirdHeight(Config config, double height1, double height2)
    {
        var uiItem = config.UiItem ?? new();

        uiItem.MainGirdHeight1 = (int)(height1 + 0.1);
        uiItem.MainGirdHeight2 = (int)(height2 + 0.1);

        return 0;
    }

    #endregion UIItem
}
