using System.Text;
using DiscordClientProxy.Utilities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DiscordClientProxy.Controllers;

[ApiController]
[Route("/")]
public class IndexController : ControllerBase
{
    private readonly ILogger<IndexController> _logger;


    public IndexController(ILogger<IndexController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/")]
    [HttpGet("/app")]
    [HttpGet("/login")]
    [HttpGet("/register")]
    [HttpGet("/channels/{*:_}")]
    [HttpGet("/store")]
    [HttpGet("/store/{*:_}")]
    [HttpGet("/settings")]
    [HttpGet("/settings/{*:_}")]
    [HttpGet("/users/{*:_}")]
    [EnableCors]
    public async Task<object> Home(string? _)
    {
        if (AssetCache.Instance.ClientPageHtml is not null)
            return File(Encoding.UTF8.GetBytes(AssetCache.Instance.ClientPageHtml), "text/html");
        return File(Encoding.UTF8.GetBytes(await TestClientBuilder.BuildClientPage()), "text/html");
    }

    [HttpGet("/developers")]
    [HttpGet("/developers/{*:_}")]
    public async Task<object> Developers()
    {
        if (AssetCache.Instance.DevPageHtml is not null)
            return File(Encoding.UTF8.GetBytes(AssetCache.Instance.DevPageHtml), "text/html");
        return File(Encoding.UTF8.GetBytes(await TestClientBuilder.BuildDevPage()), "text/html");
    }

    [HttpGet("/dbg")]
    public async Task<object> Debug()
    {
        return JsonConvert.SerializeObject(new
        {
            AssetCache.Instance.ClientPageHtml,
            AssetCache.Instance.DevPageHtml,
            asset_cache = AssetCache.Instance.asset_cache.Select(x => new KeyValuePair<string, string>(x.Key, Encoding.UTF8.GetString(x.Value)))
            //AssetCache.Instance.resource_cache
        }, Formatting.Indented);
    }
}