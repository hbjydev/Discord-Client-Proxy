using System.Text;
using DiscordClientProxy.Utilities;

namespace DiscordClientProxy;

public class AssetProvider
{
    

    public static async Task<byte[]?> GetFromNetwork(string asset)
    {
        var url = "https://discord.com/assets/" + asset;
        var path = $"{Configuration.Instance.AssetCacheLocationResolved}/{asset}";

        Console.WriteLine($"[Asset cache] Downloading {url}");

        using var hc = new HttpClient();
        var resp = await hc.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return null;
        var bytes = await resp.Content.ReadAsByteArrayAsync();

        

       
        return bytes;
    }
    public static async Task StreamToDiskAsync(string path, string url)
    {
        //var url = "https://discord.com/assets/" + asset;
        //var path = $"{Configuration.Instance.AssetCacheLocationResolved}/{asset}";
        Console.WriteLine($"[Asset cache] Streaming {url} -> {path}");

        using var hc = new HttpClient();
        var resp = await hc.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return;
        var netStream = await resp.Content.ReadAsStreamAsync();
        var fileStream = File.OpenWrite(path);
        await netStream.CopyToAsync(fileStream);
        await fileStream.FlushAsync();
        fileStream.Close();
        netStream.Close();
    }
}