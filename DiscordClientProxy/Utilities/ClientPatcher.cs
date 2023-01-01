using DiscordClientProxy.Classes;
using DiscordClientProxy.ClientPatches;
using DiscordClientProxy.ClientPatches.Branding;
using DiscordClientProxy.ClientPatches.CustomisationPatches;

namespace DiscordClientProxy.Utilities;

public class ClientPatcher
{
    public static ClientPatch[] ClientPatches =
    {
        new RemoveSourceMapUrlPatch(), // Remove source map urls, saves some requests
        new AlternateSentryPatch(), // Move sentry to ours, as to not flood Discord.com's sentry
        new GlobalEnvWarningPatch(), // Clarify global env warning to hint misconfiguration
        new PlainTextGatewayPatch(), // Use plaintext gateway, useful in debugging
        new NoXssWarningPatch(), // Disable the self-xss warnings (anti-copy-paste)
        new GatewayImmediateReconnectPatch(), // Remove reconnect delay in gateway
        new KeepLocalStoragePatch(), // Prevent client from clearing localStorage 
        new NoQrLoginPatch(), // Remove QR login
        new FastIdentifyPatch(),
        //branding
        new BrandingLogoPatch(),
        new BrandingPremiumPatch(),
        new BrandingNamePatch(),
        new BrandingGuildReferencePatch(),
        
        //extras
        new ChangelogPatch()
    };

    public static void EnsureConfigPopulated()
    {
        Console.WriteLine("[ClientPatcher] Populating config with patches...");
        foreach (var patch in ClientPatches)
        {
            if (Configuration.Instance.Client.DebugOptions.Patches.ContainsKey(patch.GetType().Name)) continue;
            Console.WriteLine($"[ClientPatcher] Adding patch {patch.GetType().Name} to config with default value {patch.IsEnabledByDefault}");
            Configuration.Instance.Client.DebugOptions.Patches.TryAdd(patch.GetType().Name, patch.IsEnabledByDefault);
            Configuration.Instance.Save();
        }
    }

    public static async Task PatchFile(string path)
    {
        Console.WriteLine($"[ClientPatcher] Applying patches to {path}...");
        var content = await File.ReadAllTextAsync(path);
        content = await Patch(content);
        await File.WriteAllTextAsync(path, content);
    }

    public static async Task<string> Patch(string content)
    {
        foreach (var patch in ClientPatches)
        {
            //make sure its definitely in there!
            if (!Configuration.Instance.Client.DebugOptions.Patches.ContainsKey(patch.GetType().Name))
            {
                Configuration.Instance.Client.DebugOptions.Patches.TryAdd(patch.GetType().Name, patch.IsEnabledByDefault);
                Configuration.Instance.Save();
            }

            if (Configuration.Instance.Client.DebugOptions.Patches.TryGetValue(patch.GetType().Name, out var enabled) && enabled)
                content = await patch.ApplyPatch(content);
        }

        return content;
    }
}