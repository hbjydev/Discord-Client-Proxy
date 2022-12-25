using System.Text.RegularExpressions;
using DiscordClientProxy.Classes;

namespace DiscordClientProxy.ClientPatches.Branding;

public class BrandingNamePatch : ClientPatch
{
    public override async Task<string> ApplyPatch(string content)
    {
        if (!content.Contains("Discord")) return content;
        Console.WriteLine($"[ClientPatch:{GetType().Name}] Applying patch...");
        content = content.Replace(" Discord ", $" {Configuration.Instance.InstanceName} ");
        content = content.Replace("Discord ", $"{Configuration.Instance.InstanceName} ");
        content = content.Replace(" Discord", $" {Configuration.Instance.InstanceName}");
        content = content.Replace("Discord Premium", $"{Configuration.Instance.InstanceName} Premium");
        content = content.Replace("Discord Nitro", $"{Configuration.Instance.InstanceName} Premium");
        content = content.Replace("Discord's", $"{Configuration.Instance.InstanceName}'s");
        //content = content.Replace("DiscordTag", "FosscordTag");
        content = content.Replace("*Discord*", $"*{Configuration.Instance.InstanceName}*");
        return content;
    }
}