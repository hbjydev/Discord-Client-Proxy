using System.Collections.Concurrent;
using System.Text;
using DiscordClientProxy.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace DiscordClientProxy.Controllers;

[ApiController]
[Route("/assets")]
public class AssetProxyController : ControllerBase
{
    private readonly ILogger<AssetProxyController> _logger;

    public AssetProxyController(ILogger<AssetProxyController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/assets/{*res:required}")]
    public async Task<object> Asset(string res)
    {
        //we dont have map files, so dont even bother
        if (res.EndsWith(".map")) return NotFound();

        byte[]? data = null;
        data ??= await GetFromCache(res);
        data ??= await GetFromDisk(res);
        data ??= await GetFromNetwork(res);
        
        if (data is null) return NotFound();
        return File(data, HttpUtilities.GetContentTypeByFilename(res));
    }

    private static async Task<byte[]?> GetFromDisk(string asset)
    {
        if (System.IO.File.Exists($"{Configuration.Instance.AssetCacheLocationResolved}/{asset}"))
        {
            var data = await System.IO.File.ReadAllBytesAsync($"{Configuration.Instance.AssetCacheLocationResolved}/{asset}");
            if (Configuration.Instance.Cache.Memory) AssetCache.Instance.asset_cache.TryAdd(asset, data);
            return data;
        }
        return null;
    }
    private static async Task<byte[]?> GetFromNetwork(string asset)
    {
        var url = "https://discord.com/assets/" + asset;
        var path = $"{Configuration.Instance.AssetCacheLocationResolved}/{asset}";
        
        Console.WriteLine($"[Asset cache] Downloading {url}");
        
        using var hc = new HttpClient();
        var resp = await hc.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return null;
        var bytes = await resp.Content.ReadAsByteArrayAsync();

        if (asset.EndsWith(".js") || asset.EndsWith(".css"))
        {
            bytes = Encoding.UTF8.GetBytes(
                await ClientPatcher.Patch(
                    Encoding.UTF8.GetString(bytes)
                )
            );
        }

        if (Configuration.Instance.Cache.Disk)
        {
            Console.WriteLine($"[Asset cache] Saving {url} -> {path}");
            Directory.CreateDirectory(Configuration.Instance.AssetCacheLocationResolved);
            await System.IO.File.WriteAllBytesAsync($"{Configuration.Instance.AssetCacheLocationResolved}/{asset}",
                bytes);
        }
        if (Configuration.Instance.Cache.Memory)
            AssetCache.Instance.asset_cache.TryAdd(asset, bytes);
        return bytes;
    }

    private static async Task<byte[]?> GetFromCache(string asset)
    {
        return AssetCache.Instance.asset_cache.ContainsKey(asset) ? AssetCache.Instance.asset_cache[asset] : null;
    }
}