using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Html;
using AngleSharp.Html.Parser;

namespace DiscordClientProxy.Utilities;

public class TestClientBuilder
{
    public static async Task PrefetchClient()
    {
/*
        var originalHtml = await GetOriginalHtml();
        var lines = originalHtml.Split("\n");

        var preloadScripts = await GetPreloadScripts(lines);
        var mainScripts = await GetMainScripts(lines);
        var css = await GetCss(lines);

        Console.WriteLine($"[TestClientBuilder] Found {preloadScripts.Length + mainScripts.Length + css.Length} assets");
        preloadScripts = preloadScripts.Where(x => !File.Exists($"{Configuration.Instance.AssetCacheLocationResolved}/{x.Replace("/assets/", "")}")).ToArray();
        mainScripts = mainScripts.Where(x => !File.Exists($"{Configuration.Instance.AssetCacheLocationResolved}/{x.Replace("/assets/", "")}")).ToArray();
        css = css.Where(x => !File.Exists($"{Configuration.Instance.AssetCacheLocationResolved}/{x.Replace("/assets/", "")}")).ToArray();

        Console.WriteLine($"[TestClientBuilder] Found {preloadScripts.Length + mainScripts.Length + css.Length} assets");
        //await Task.WhenAll(mainScripts.Select(async x => { await AssetCache.GetFromNetwork(x.Replace("/assets/", "")); }));
        //await Task.WhenAll(css.Select(async x => { await AssetCache.GetFromNetwork(x.Replace("/assets/", "")); }));
        var throttler = new SemaphoreSlim(System.Environment.ProcessorCount * 8);
        var tasks = preloadScripts.Select(async x =>
        {
            await throttler.WaitAsync();
            //await AssetCache.GetFromNetwork(x.Replace("/assets/", ""));
            throttler.Release();
        }).ToList();
        await Task.WhenAll(tasks);
        await AssetCache.FindEmojiMatches();
        */
    }

    public static async Task<byte[]> BuildClientPage()
    {
        if (Configuration.Instance.Cache.ReuseHtml && MemoryStore.ClientPageHtml != null)
            return MemoryStore.ClientPageHtml;
        Console.WriteLine("[TestClientBuilder] Building client page...");
        //var html = "";
        var html = await File.ReadAllTextAsync(RuntimeEnvironment.BinDir + "/Resources/Pages/index.html");

        html = AddScripts(html);
        html = await AddDebugUtils(html);
        //add welcome screen
        html +=
            (await File.ReadAllTextAsync(RuntimeEnvironment.BinDir + "/Resources/Private/WelcomeScreenWrapper.html"))
            .Replace("<!-- content -->",
                await File.ReadAllTextAsync(RuntimeEnvironment.BinDir +
                                            "/Resources/Overridable/WelcomeScreen/index.html"));
        MemoryStore.ClientPageHtml = Encoding.UTF8.GetBytes(html);
        return MemoryStore.ClientPageHtml;
    }


    public static async Task<byte[]> BuildDevPage()
    {
        return Array.Empty<byte>();
        /*if(Configuration.Instance.Cache.ReuseHtml && MemoryStore.DevPageHtml != null) return MemoryStore.DevPageHtml;
        Console.WriteLine("[TestClientBuilder] Building dev portal page...");
        var html = await File.ReadAllTextAsync(RuntimeEnvironment.BinDir + "/Resources/Pages/developers.html");

        var originalHtml = await GetOriginalHtml("/developers");
        var lines = originalHtml.Split("\n");

        var preloadScripts = await GetPreloadScripts(lines);
        var mainScripts = await GetMainScripts(lines);
        var css = await GetCss(lines);

        if (Configuration.Instance.Client.AddPrefetchTags)
            html = html.Replace("<!--prefetch_script-->",
                string.Join("\n", preloadScripts.Select(x => $"<link rel=\"prefetch\" as=\"script\" href=\"{x}\" />")));
        html = html.Replace("<!--client_script-->",
            string.Join("\n", mainScripts.Select(x => $"<script src=\"{x}\"></script>")));
        html = html.Replace("<!--client_css-->",
            string.Join("\n", css.Select(x => $"<link rel=\"stylesheet\" href=\"{x}\" />")));


        //inject debug utilities
        var debugOptions = Configuration.Instance.Client.DebugOptions;
        if (debugOptions.DumpWebsocketTrafficToBrowserConsole)
            html = html.Replace("<!-- preload plugin marker -->",
                await File.ReadAllTextAsync(RuntimeEnvironment.BinDir + "/Resources/Private/Injections/WebSocketDataLog.html") +
                "\n<!-- preload plugin marker -->");
        if (debugOptions.DumpWebsocketTraffic)
            html = html.Replace("<!-- preload plugin marker -->",
                await File.ReadAllTextAsync(RuntimeEnvironment.BinDir + "/Resources/Private/Injections/WebSocketDumper.html") +
                "\n<!-- preload plugin marker -->");
        
        MemoryStore.DevPageHtml = html;
        return html;*/

        //return Encoding.UTF8.GetBytes(html);
    }
    
    private static string AddScripts(string html)
    {
        if (Configuration.Instance.Client.AddPrefetchTags)
            html = html.Replace("<!--prefetch_script-->",
                string.Join("\n",
                    MemoryStore.ClientPreloadScripts.Select(
                        x => $"<link rel=\"prefetch\" as=\"script\" href=\"{x}\" />")));
        html = html.Replace("<!--client_script-->",
            string.Join("\n", MemoryStore.ClientScripts.Select(x => $"<script src=\"{x}\"></script>")));
        html = html.Replace("<!--client_css-->",
            string.Join("\n", MemoryStore.ClientStylesheets.Select(x => $"<link rel=\"stylesheet\" href=\"{x}\" />")));
        return html;
    }

    private static async Task<string> AddDebugUtils(string html)
    {
        
        //inject debug utilities
        var debugOptions = Configuration.Instance.Client.DebugOptions;
        if (debugOptions.DumpWebsocketTrafficToBrowserConsole)
            html = html.Replace("<!-- preload plugin marker -->",
                await File.ReadAllTextAsync(RuntimeEnvironment.BinDir +
                                            "/Resources/Private/Injections/WebSocketDataLog.html") +
                "\n<!-- preload plugin marker -->");
        if (debugOptions.DumpWebsocketTraffic)
            html = html.Replace("<!-- preload plugin marker -->",
                await File.ReadAllTextAsync(RuntimeEnvironment.BinDir +
                                            "/Resources/Private/Injections/WebSocketDumper.html") +
                "\n<!-- preload plugin marker -->");
        return html;
    }
}