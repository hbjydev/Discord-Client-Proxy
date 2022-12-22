namespace DiscordClientProxy.Utilities;

public class TestClientBuilder
{
    private async static Task<string> GetOriginalHtml()
    {
        var client = new HttpClient();
        var targetVer = Configuration.Instance.Version;
        if (targetVer.Contains("latest"))
        {
            var response = await client.GetAsync(
                targetVer == "latest "
                    ? "https://discordapp.com/app"
                    : targetVer == "latest-ptb"
                        ? "https://ptb.discordapp.com/app"
                        : "https://canary.discordapp.com/app"
            );
            var html = await response.Content.ReadAsStringAsync();
            return html;
        }

        if(Directory.Exists(Configuration.Instance.AssetCacheLocationResolved) && File.Exists(Path.Combine(Configuration.Instance.AssetCacheLocationResolved, "index.html")))
        {
            return File.ReadAllText(Path.Combine(Configuration.Instance.AssetCacheLocationResolved, "index.html"));
        }

        throw new Exception("Could not find index.html");
    }

    public async static Task BuildClientPage()
    {
        var html = await File.ReadAllTextAsync(Environment.BinDir + "/Resources/Pages/index-updated.html");

        //inject debug utilities
        var debugOptions = Configuration.Instance.Client.DebugOptions;
        if (debugOptions.DumpWebsocketTrafficToBrowserConsole)
            html = html.Replace("<!-- preload plugin marker -->",
                await File.ReadAllTextAsync(Environment.BinDir +"Resources/Injections/WebSocketDataLog.html") +
                "\n<!-- preload plugin marker -->");
        if (debugOptions.DumpWebsocketTraffic)
            html = html.Replace("<!-- preload plugin marker -->",
                await File.ReadAllTextAsync(Environment.BinDir +"Resources/Injections/WebSocketDumper.html") +
                "\n<!-- preload plugin marker -->");

        AssetCache.Instance.ClientPageHtml = html;
    }

    public async static Task BuildDevPage()
    {
        var html = await File.ReadAllTextAsync("Resources/Pages/index-updated.html");

        //inject debug utilities
        var debugOptions = Configuration.Instance.Client.DebugOptions;
        if (debugOptions.DumpWebsocketTrafficToBrowserConsole)
            html = html.Replace("<!-- preload plugin marker -->",
                await File.ReadAllTextAsync("Resources/Private/Injections/WebSocketDataLog.html") +
                "\n<!-- preload plugin marker -->");
        if (debugOptions.DumpWebsocketTraffic)
            html = html.Replace("<!-- preload plugin marker -->",
                await File.ReadAllTextAsync("Resources/Private/Injections/WebSocketDumper.html") +
                "\n<!-- preload plugin marker -->");

        AssetCache.Instance.ClientPageHtml = html;
    }
    
}