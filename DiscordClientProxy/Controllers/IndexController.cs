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
    [EnableCors]
    public async Task<object> Home(string? _)
    {
        if (AssetCache.Instance.ClientPageHtml is null) await TestClientBuilder.BuildClientPage();

        return File(Encoding.UTF8.GetBytes(AssetCache.Instance.ClientPageHtml), "text/html");
    }

    [HttpGet("/developers")]
    [HttpGet("/developers/{*:_}")]
    public async Task<object> Developers()
    {
        if (AssetCache.Instance.DevPageHtml is null) await TestClientBuilder.BuildDevPage();

        return File(Encoding.UTF8.GetBytes(AssetCache.Instance.DevPageHtml), "text/html");
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