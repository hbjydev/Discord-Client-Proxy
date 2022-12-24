using System.Text.RegularExpressions;
using DiscordClientProxy.Classes;

namespace DiscordClientProxy.ClientPatches.Branding;

public class BrandingGuildReferencePatch : ClientPatch
{
    public override async Task<string> ApplyPatch(string content)
    {
        if (!content.Contains("Server")) return content;
        Console.WriteLine($"[ClientPatch:{GetType().Name}] Applying patch...");
        var variations = new Dictionary<string, string>()
        {
            {"\"Server\"", "\"Guild\""},
            {"\"Server ", "\"Guild "},
            {" Server\"", " Guild\""},
            {" Server ", " Guild "},

            {"\"Server.\"", "\"Guild.\""},
            {" Server.\"", " Guild.\""},

            {"\"Server,\"", "\"Guild,\""},
            {" Server,\"", " Guild,\""},
            {" Server,", " Guild,"},

            {"\"Servers\"", "\"Guilds\""},
            {"\"Servers ", "\"Guilds "},
            {" Servers\"", " Guilds\""},
            {" Servers ", " Guilds "},

            {"\"Servers.\"", "\"Guilds.\""},
            {" Servers.\"", " Guilds,\""},

            {"\"Servers,\"", "\"Guilds,\""},
            {" Servers,\"", " Guilds,\""},
            {" Servers,", " Guilds,"},

            {"\nServers", "\nGuilds"},
        };
        var count = variations.Count;
        for (var i = 0; i < count; i++)
        {
            variations.Add(variations.ElementAt(i).Key.ToLower(), variations.ElementAt(i).Value.ToLower());
        }
        foreach (var variation in variations)
        {
            content = content.Replace(variation.Key, variation.Value);
        }
        return content;
    }
}