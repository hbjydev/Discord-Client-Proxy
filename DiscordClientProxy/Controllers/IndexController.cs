using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DiscordClientProxy.Utilities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
        return File(await TestClientBuilder.BuildClientPage(), "text/html");
    }

    [HttpGet("/developers")]
    [HttpGet("/developers/{*:_}")]
    public async Task<object> Developers()
    {
        return File(await TestClientBuilder.BuildDevPage(), "text/html");
    }

    [HttpGet("/dbg")]
    public async Task<object> Debug()
    {
        return JsonSerializer.Serialize(new
        {
            ClientPageHtml = Encoding.UTF8.GetString(MemoryStore.ClientPageHtml??Array.Empty<byte>()),
            DevPageHtml = Encoding.UTF8.GetString(MemoryStore.DevPageHtml??Array.Empty<byte>()),
            asset_cache = MemoryStore.asset_cache.Select(x => new KeyValuePair<string, string>(x.Key, Encoding.UTF8.GetString(x.Value)))
            //AssetCache.Instance.resource_cache
        }, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }
}