using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using DiscordClientProxy.Interfaces;
using DiscordClientProxy.Utilities;

namespace DiscordClientProxy.StartupTasks;

public class PrefetchClientRecursiveTask : IStartupTask
{
    public int Order { get; } = 11;

    public async Task ExecuteAsync()
    {
        //await TestClientBuilder.PrefetchClient();
        if (!Configuration.Instance.Cache.StartupCacheOptions.DownloadOnStart) return;
        if (!Configuration.Instance.Cache.DownloadAssetsRecursive) return;
        if (!Configuration.Instance.Version.Contains("latest")) return;
        for (int i = 0; i < Configuration.Instance.Cache.RecursiveDownloadDepth; i++)
        {
            Console.WriteLine($"[Startup/PrefetchClientResursiveTask] Downloading assets recursively, depth {i + 1} of {Configuration.Instance.Cache.RecursiveDownloadDepth}");
            Console.WriteLine("[Startup/PrefetchClientResursiveTask] ==> Enumerating assets...");
            var files = TieredAssetStore.EnumerateAssets().Where(x => x.EndsWith(".js") || x.EndsWith(".css"));
            Console.WriteLine("[Startup/PrefetchClientResursiveTask] ==> Reading assets...");
            var assettasks = files
                .Select(x => Task.Factory.StartNew(() => TieredAssetStore.GetAsset(x))).ToList();
            await Task.WhenAll(assettasks);
            var fileContentsAsBytes = assettasks.Select(x => x.Result.Result);
            Console.WriteLine("[Startup/PrefetchClientResursiveTask] ==> Reading assets as strings...");
            var fileContents = fileContentsAsBytes.Select(x => Encoding.UTF8.GetString(x));
            Console.WriteLine("[Startup/PrefetchClientResursiveTask] ==> Parsing assets...");
            
        }
        //await FetchAssets(Configuration.Instance.Cache.AppBaseUri);
        //await FetchAssets(Configuration.Instance.Cache.DevBaseUri);
    }

    private static async Task FetchAssets(string url)
    {
        Console.WriteLine($"[Startup/PrefetchClientTask] Downloading app HTML from {url}");
        var html = (await GetHtmlFormatted(url)).Split('\n');
        Console.WriteLine("[Startup/PrefetchClientTask] Fetching missing assets...");
        var assets = new List<string>();
        MemoryStore.ClientScripts.Clear();
        MemoryStore.ClientPreloadScripts.Clear();
        MemoryStore.ClientStylesheets.Clear();
        //css
        var cssHtml = html.Where(x => x.Contains("<link rel=\"stylesheet\"")).ToList();
        foreach (var script in cssHtml)
        {
            var match = Regex.Match(script, "href=\"(.*?)\"");
            if (match.Success)
            {
                assets.Add(match.Groups[1].Value);
                MemoryStore.ClientStylesheets.Add(match.Groups[1].Value);
            }
        }

        //scripts
        var mainScriptHtml = html.Where(x => x.Contains("<script src=")).ToList();
        foreach (var script in mainScriptHtml)
        {
            var match = Regex.Match(script, "src=\"(.*?)\"");
            if (match.Success)
            {
                assets.Add(match.Groups[1].Value);
                MemoryStore.ClientScripts.Add(match.Groups[1].Value);
            }
        }

        //preload
        var prefetchScriptHtml = html.Where(x => x.Contains("link rel=\"prefetch\" as=\"script\"")).ToList();
        foreach (var script in prefetchScriptHtml)
        {
            var match = Regex.Match(script, "href=\"(.*?)\"");
            if (match.Success)
            {
                assets.Add(match.Groups[1].Value);
                MemoryStore.ClientPreloadScripts.Add(match.Groups[1].Value);
            }
        }

        var assettasks = assets
            .Select(x => Task.Factory.StartNew(() => TieredAssetStore.GetAsset(x))).ToList();
        await Task.WhenAll(assettasks);
        //download
        // foreach (var asset in assets)
        // {
        //     var assetUrl = Configuration.Instance.Cache.AssetBaseUri + asset.Replace("/assets/", "");
        //     Console.WriteLine($"[Startup/PrefetchClientTask] Downloading {assetUrl}");
        //     // await GetHtmlFormatted(assetUrl);
        // }

        var files = Directory.GetFiles(Configuration.Instance.AssetCacheLocationResolved).Where(x => x.EndsWith(".js") || x.EndsWith(".css"));
        foreach (var file in files)
        {

            DownloadFile(file);

        }
    }

    private static async Task DownloadFile(string asset)
    {
        var content = Encoding.UTF8.GetString(await TieredAssetStore.GetAsset(asset));
        var assets = FindMoreAssets(content);
        assets = assets.Where(x => !File.Exists($"{Configuration.Instance.AssetCacheLocationResolved}/{x}")).ToList();
        if (assets.Count > 0)
        {
            Console.WriteLine($"[ClientPatcher] Found {assets.Count} assets to fetch");
        }
    }

    private static async Task<string> GetHtmlFormatted(string url)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();
        var document = new HtmlParser().ParseDocument(html);
        var sw = new StringWriter();
        document.ToHtml(sw, new PrettyMarkupFormatter());
        return sw.ToString();
    }
    
    //no async here :c
    public static List<string> FindMoreAssets(string content)
    {
        string pattern = @"\.exports=.\..\+\""(.*?\..{0,5})\""";
        var matches = Regex.Matches(content, pattern);
        var assets = new List<string>();
        foreach (Match m in matches)
        {
            assets.Add(m.Groups[1].Value);
        }

        return assets;
    }
}