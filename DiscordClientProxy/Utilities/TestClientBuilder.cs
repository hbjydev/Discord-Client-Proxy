using AngleSharp.Html;
using AngleSharp.Html.Parser;

namespace DiscordClientProxy.Utilities;

public class TestClientBuilder
{
    private static async Task<string> GetOriginalHtml()
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
            //format html
            var parser = new HtmlParser();

            var document = parser.ParseDocument(html);

            var sw = new StringWriter();
            document.ToHtml(sw, new PrettyMarkupFormatter());
            return sw.ToString();
        }

        if(Directory.Exists(Configuration.Instance.AssetCacheLocationResolved) && File.Exists(Path.Combine(Configuration.Instance.AssetCacheLocationResolved, "index.html")))
        {
            return File.ReadAllText(Path.Combine(Configuration.Instance.AssetCacheLocationResolved, "index.html"));
        }

        throw new Exception("Could not find index.html");
    }

    public static async Task BuildClientPage()
    {
        var html = await File.ReadAllTextAsync(Environment.BinDir + "/Resources/Pages/index.html");
        var originalHtml = await GetOriginalHtml();
        var lines = originalHtml.Split("\n");
        html = html.Replace("<!--prefetch_script-->",
            string.Join("\n", lines.Where(x => x.Contains("link rel=\"prefetch\" as=\"script\""))));
        html = html.Replace("<!--client_script-->",
            string.Join("\n", lines.Where(x => x.Contains("<script src="))));
        html = html.Replace("<!--client_css-->",
            string.Join("\n", lines.Where(x => x.Contains("link rel=\"stylesheet\""))));
        html = html.Replace("integrity", "hashes");
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

    public static async Task BuildDevPage()
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