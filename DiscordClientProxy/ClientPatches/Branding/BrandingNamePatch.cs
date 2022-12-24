using System.Text.RegularExpressions;
using DiscordClientProxy.Classes;

namespace DiscordClientProxy.ClientPatches.Branding;

public class BrandingNamePatch : ClientPatch
{
    public override async Task<string> ApplyPatch(string content)
    {
        if (!content.Contains("Discord")) return content;
        Console.WriteLine($"[ClientPatch:{GetType().Name}] Applying patch...");
        content = content.Replace(" Discord ", " ${INSTANCE_NAME} ");
        content = content.Replace("Discord ", "${INSTANCE_NAME} ");
        content = content.Replace(" Discord", " ${INSTANCE_NAME}");
        content = content.Replace("Discord Premium", "${INSTANCE_NAME} Premium");
        content = content.Replace("Discord Nitro", "${INSTANCE_NAME} Premium");
        content = content.Replace("Discord's", "${INSTANCE_NAME}'s");
        //content = content.Replace("DiscordTag", "FosscordTag");
        content = content.Replace("*Discord*", "*${INSTANCE_NAME}*");
        return content;
    }
}