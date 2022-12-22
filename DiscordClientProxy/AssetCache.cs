using System.Collections.Concurrent;

namespace DiscordClientProxy;

public class AssetCache
{
    public static AssetCache Instance { get; } = new();
    public readonly ConcurrentDictionary<string, byte[]> asset_cache = new();
    public readonly ConcurrentDictionary<string, byte[]> resource_cache = new();
    public string ClientPageHtml { get; set; }
    public string DevPageHtml { get; set; }
    
}