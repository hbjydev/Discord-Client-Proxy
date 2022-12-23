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

        if (AssetCache.Instance.asset_cache.ContainsKey("res"))
        {
            byte[] result = AssetCache.Instance.asset_cache[res];
            return File(result, HttpUtilities.GetContentTypeByFilename(res));
        }

        using var hc = new HttpClient();
        var resp = await hc.GetAsync("https://discord.com/assets/" + res);
        if (!resp.IsSuccessStatusCode) return NotFound();
        var bytes = await resp.Content.ReadAsByteArrayAsync();

        if (res.EndsWith(".js") || res.EndsWith(".css"))
        {
            bytes = Encoding.UTF8.GetBytes(
                PatchClient(
                    Encoding.UTF8.GetString(bytes)
                )
            );
        }

        if (Configuration.Instance.Cache.Disk)
        {
            Directory.CreateDirectory(Configuration.Instance.AssetCacheLocationResolved);
            await System.IO.File.WriteAllBytesAsync($"{Configuration.Instance.AssetCacheLocationResolved}/{res}",
                bytes);
        }

        AssetCache.Instance.asset_cache.TryAdd(res, bytes);

        return File(bytes, HttpUtilities.GetContentTypeByFilename(res));
        //return NotFound();
    }


    public static string PatchClient(string str)
    {
        TestClientPatchOptions patchOptions = Configuration.Instance.Client.DebugOptions.Patches;
        str = str.Replace("//# sourceMappingURL=", "//# disabledSourceMappingURL=");
        str = str.Replace("https://fa97a90475514c03a42f80cd36d147c4@sentry.io/140984",
            "https://6bad92b0175d41a18a037a73d0cff282@sentry.thearcanebrony.net/12");
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