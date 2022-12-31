using System.Collections.Concurrent;
using System.Text;
using DiscordClientProxy.Utilities;

namespace DiscordClientProxy;

public class AssetCache
{
    public readonly ConcurrentDictionary<string, byte[]> asset_cache = new();
    public readonly ConcurrentDictionary<string, byte[]> resource_cache = new();
    public static AssetCache Instance { get; } = new();
    public string ClientPageHtml { get; set; }
    public string DevPageHtml { get; set; }


    public static async Task<byte[]?> GetFromDisk(string asset)
    {
        if (File.Exists($"{Configuration.Instance.AssetCacheLocationResolved}/{asset}"))
        {
            var data = await File.ReadAllBytesAsync($"{Configuration.Instance.AssetCacheLocationResolved}/{asset}");
            if (Configuration.Instance.Cache.Memory) Instance.asset_cache.TryAdd(asset, data);
            return data;
        }

        return null;
    }

    public static async Task<byte[]?> GetFromNetwork(string asset)
    {
        var url = "https://discord.com/assets/" + asset;
        var path = $"{Configuration.Instance.AssetCacheLocationResolved}/{asset}";

        Console.WriteLine($"[Asset cache] Downloading {url}");

        using var hc = new HttpClient();
        var resp = await hc.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return null;
        var bytes = await resp.Content.ReadAsByteArrayAsync();

        if (asset.EndsWith(".js") || asset.EndsWith(".css"))
            bytes = Encoding.UTF8.GetBytes(
                await ClientPatcher.Patch(
                    Encoding.UTF8.GetString(bytes)
                )
            );

        if (Configuration.Instance.Cache.Disk)
        {
            Console.WriteLine($"[Asset cache] Saving {url} -> {path}");
            Directory.CreateDirectory(Configuration.Instance.AssetCacheLocationResolved);
            await File.WriteAllBytesAsync($"{Configuration.Instance.AssetCacheLocationResolved}/{asset}",
                bytes);
        }

        if (Configuration.Instance.Cache.Memory)
            Instance.asset_cache.TryAdd(asset, bytes);
        return bytes;
    }

    public static async Task<byte[]?> GetFromCache(string asset)
    {
        return Instance.asset_cache.ContainsKey(asset) ? Instance.asset_cache[asset] : null;
    }
}