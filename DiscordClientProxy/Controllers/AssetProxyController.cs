using System.Collections.Concurrent;
using System.Text;
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
                PatchClient(
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

    public static string PatchClient(string str)
    {
        TestClientPatchOptions patchOptions = Configuration.Instance.Client.DebugOptions.Patches;
        //required patches
        // - remove source map urls, saves some requests
        str = str.Replace("//# sourceMappingURL=", "//# disabledSourceMappingURL=");
        // - move sentry to mine, as to not flood Discord.com's sentry
        str = str.Replace("https://fa97a90475514c03a42f80cd36d147c4@sentry.io/140984",
            "https://6bad92b0175d41a18a037a73d0cff282@sentry.thearcanebrony.net/12");
        // - Modify global env warning
        str = str.Replace("Global environment variables not set!", "Global environment variables not set!\\n[DiscordClientProxy] Make sure your reverse proxy is configured correctly!");
        
        if (patchOptions.GatewayPlaintext)
        {
            str = str.Replace("e.isDiscordGatewayPlaintextSet=function(){0;return!1};",
                "e.isDiscordGatewayPlaintextSet = function() { return true };");
        }

        if (patchOptions.NoXssWarning)
        {
            str = str.Replace("console.log(\"%c\"+n.SELF_XSS_", "console.valueOf(n.SELF_XSS_");
            str = str.Replace("console.log(\"%c\".concat(n.SELF_XSS_", "console.valueOf(console.valueOf(n.SELF_XSS_");
        }

        if (patchOptions.GatewayImmediateReconnect)
        {
            str = str.Replace("nextReconnectIsImmediate=!1", "nextReconnectIsImmediate = true");
        }

        return str;
    }
}