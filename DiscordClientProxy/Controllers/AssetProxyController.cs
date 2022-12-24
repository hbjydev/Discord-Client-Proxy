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
        data ??= await AssetCache.GetFromCache(res);
        data ??= await AssetCache.GetFromDisk(res);
        data ??= await AssetCache.GetFromNetwork(res);
        
        if (data is null) return NotFound();
        return File(data, HttpUtilities.GetContentTypeByFilename(res));
    }
}