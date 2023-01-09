using System.Collections.Concurrent;
using System.Text;
using DiscordClientProxy.Utilities;

namespace DiscordClientProxy;

public class MemoryStore
{
    public static readonly ConcurrentDictionary<string, byte[]> asset_cache = new();
    public static readonly ConcurrentDictionary<string, byte[]> resource_cache = new();
    public static string? ClientPageHtml { get; set; }
    public static string? DevPageHtml { get; set; }
    
    public static List<string> PreloadScripts { get; set; } = new();
    public static List<string> Stylesheets { get; set; } = new();
    public static List<string> Scripts { get; set; } = new();
}

public class TieredAssetStore
{
    public static async Task<byte[]> GetAsset(string asset)
    {
        byte[] data;
        if (Configuration.Instance.Cache.Memory && MemoryStore.asset_cache.TryGetValue(asset, out data)) return data;
        if (Configuration.Instance.Cache.Disk && File.Exists(Configuration.Instance.AssetCacheLocationResolved + asset))
            data = await File.ReadAllBytesAsync(Configuration.Instance.AssetCacheLocationResolved + asset);
        else
        {
            using HttpClient client = new();
            Console.WriteLine($"Downloading asset: {Configuration.Instance.Cache.AssetBaseUri + asset}");
            data = await client.GetByteArrayAsync(Configuration.Instance.Cache.AssetBaseUri + asset);
            if (asset.EndsWith(".js") || asset.EndsWith(".css"))
                data = Encoding.UTF8.GetBytes(
                    await ClientPatcher.Patch(
                        Encoding.UTF8.GetString(data)
                    )
                );
        }

        await PromoteAsset(asset, data);
        return data;
    }

    private static async Task PromoteAsset(string asset, byte[] data)
    {
        if (Configuration.Instance.Cache.Memory && !MemoryStore.asset_cache.ContainsKey(asset))
            MemoryStore.asset_cache.TryAdd(asset, data);
        if (Configuration.Instance.Cache.Disk && !File.Exists(Configuration.Instance.AssetCacheLocationResolved + asset)) 
            await File.WriteAllBytesAsync(Configuration.Instance.AssetCacheLocationResolved + asset, data);
    }
}