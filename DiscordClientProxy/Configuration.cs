using Newtonsoft.Json;

namespace DiscordClientProxy;

public class Configuration
{
    public static Configuration Instance { get; private set; } = new();

    public string Version { get; set; } = "latest";
    public string AssetCacheLocation { get; set; } = "assets_cache/$VERSION/";
    public string InstanceName { get; set; } = "Fosscord";

    public ClientOptions Client { get; set; } = new();
    public CacheOptions Cache { get; set; } = new();
    public DebugOptions Debug { get; set; } = new();

    [JsonIgnore]
    public string AssetCacheLocationResolved => AssetCacheLocation.Replace("$VERSION", Version);

    //This must be delayed until after working dir is set!
    public static void Load()
    {
        Instance = (File.Exists(Environment.BaseDir + "/config.json")
            ? JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Environment.BaseDir + "/config.json"))
            : new Configuration()) ?? new Configuration();
        Instance.Save();
    }

    public void Save()
    {
        File.WriteAllText(Environment.BaseDir + "/config.json", JsonConvert.SerializeObject(Instance, Formatting.Indented));
    }
}

public class DebugOptions
{
    public string? ClientEnvProxyUrl { get; set; } = null;
}

public class CacheOptions
{
    public bool Disk { get; set; } = true;
    public bool Memory { get; set; } = true;
    public bool MemoryCacheHtml { get; set; } = true;
    public bool WipeOnStart { get; set; } = false;
    public bool DownloadAssetsRecursive { get; set; } = false;
    public bool PreloadFromDisk { get; set; } = true;
    public bool PreloadFromWeb { get; set; } = true;
}

public class ClientOptions
{
    public bool AddPrefetchTags { get; set; } = true;
    public ClientDebugOptions DebugOptions { get; set; } = new();
}

public class ClientDebugOptions
{
    public bool DumpWebsocketTrafficToBrowserConsole { get; set; } = false;
    public bool DumpWebsocketTraffic { get; set; } = false;
    public Dictionary<string, bool> Patches { get; set; } = new();
}