using DiscordClientProxy.Classes;

namespace DiscordClientProxy.ClientPatches.CustomisationPatches;

public class ChangelogPatch : ClientPatch
{
    public override async Task<string> ApplyPatch(string content)
    {
        if (!content.Contains("exports='---changelog---")) return content;
        Console.WriteLine($"[ClientPatch:{GetType().Name}] Applying patch...");
        var changelog = content.Split("\n").First(x => x.Contains("exports='---changelog---"));
        changelog = changelog[(changelog.IndexOf("'") + 1)..^1];
        await DumpChangelog(changelog);
        var newContent = await RebuildChangelog(changelog);
        content = content.Replace(changelog, newContent);
        return content;
    }

    private async Task<string> RebuildChangelog(string changelog)
    {
        var locales = await SplitLocales(changelog);
        var newChangelog = "";
        if (File.Exists("changelogs/changelog.override.txt")) return await File.ReadAllTextAsync("changelogs/changelog.override.txt");
        foreach (var locale in locales)
        {
            var localeChangelog = File.Exists($"changelogs/changelog_{locale.Key}.override.txt") 
                ? await File.ReadAllTextAsync($"changelogs/changelog_{locale.Key}.override.txt") 
                : await File.ReadAllTextAsync($"changelogs/changelog_{locale.Key}.txt");
            newChangelog += localeChangelog.Replace("\n", "\\n");
        }

        return newChangelog;
    }

    private async Task DumpChangelog(string changelog)
    {
        var locales = await SplitLocales(changelog);
        Directory.CreateDirectory("changelogs");
        await File.WriteAllTextAsync("changelogs/changelog.txt", changelog.Replace("\\n", "\n"));


        foreach (var (locale, content) in locales)
        {
            await File.WriteAllTextAsync("changelogs/changelog_" + locale + ".txt", content);
        }
    }

    private async Task<Dictionary<string, string>> SplitLocales(string changelog)
    {
        Dictionary<string, string> locales = new();
        var logs = changelog.Split("---changelog---")[1..];
        foreach (var log in logs)
        {
            var lines = log.Split("\\n");
            lines[0] = "---changelog---";
            var localeLine = lines.First(x => x.Contains("locale: "));
            var locale = localeLine.Split(": ")[1].Replace("\"", "");
            locales.Add(locale, string.Join("\n", lines));
        }

        return locales;
    }
}