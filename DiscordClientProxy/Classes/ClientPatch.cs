namespace DiscordClientProxy.Classes;

public abstract class ClientPatch
{
    public virtual bool IsEnabledByDefault { get; set; } = true;

    public async Task ApplyPatchToFile(string filePath)
    {
        Console.WriteLine($"[ClientPatch:{GetType().Name}] Applying patch to {filePath}...");
        await File.WriteAllTextAsync(filePath, await ApplyPatch(await File.ReadAllTextAsync(filePath)));
    }

    public virtual async Task<string> ApplyPatch(string content)
    {
        Console.WriteLine($"[ClientPatch:{GetType().Name}] Not implemented!");
        return content;
    }
}