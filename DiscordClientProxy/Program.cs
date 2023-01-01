using DiscordClientProxy;
using DiscordClientProxy.Utilities;
using Environment = DiscordClientProxy.Environment;

//set working dir to environment specified base directory
if (!Directory.Exists(Environment.BaseDir))
    Directory.CreateDirectory(Environment.BaseDir);

System.Environment.CurrentDirectory = Environment.BaseDir;
Configuration.Load();
//handle cache settings
if (Configuration.Instance.Cache.WipeOnStart && Directory.Exists(Configuration.Instance.AssetCacheLocationResolved))
{
    Console.WriteLine("Wiping cache...");
    //Directory.Delete(Configuration.Instance.AssetCacheLocationResolved, true);
    WipeAssetsRecursive(Configuration.Instance.AssetCacheLocationResolved);
}

void WipeAssetsRecursive(string dir)
{
    foreach (var directory in Directory.GetDirectories(dir))
    {
        WipeAssetsRecursive(directory);
    }
    foreach(var file in Directory.GetFiles(dir).Where(x=>x.EndsWith(".js") || x.EndsWith(".css")))
    {
        File.Delete(file);
        Console.WriteLine($"Deleted {file}");
    }
}

await TestClientBuilder.PrefetchClient();
if (Configuration.Instance.Cache.PreloadFromDisk && Directory.Exists(Configuration.Instance.AssetCacheLocationResolved))
    foreach (var file in Directory.GetFiles(Configuration.Instance.AssetCacheLocationResolved))
    {
        Console.WriteLine($"Preloading {file}...");
        AssetCache.Instance.asset_cache.TryAdd(new FileInfo(file).Name, File.ReadAllBytes(file));
    }

ClientPatcher.EnsureConfigPopulated();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors();
app.MapControllers();

if (Configuration.Instance.Debug.ClientEnvProxyUrl != null)
    app.MapGet("/api/_fosscord/v1/global_env", async context =>
    {
        var client = new HttpClient();
        var response = await client.GetAsync(Configuration.Instance.Debug.ClientEnvProxyUrl);
        var content = await response.Content.ReadAsStringAsync();
        await context.Response.WriteAsync(content);
    });

app.Run();