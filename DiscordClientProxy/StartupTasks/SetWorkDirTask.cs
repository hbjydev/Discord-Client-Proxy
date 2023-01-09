using DiscordClientProxy.Interfaces;

namespace DiscordClientProxy.StartupTasks;

public class SetWorkDirTask : IStartupTask
{
    public int Order { get; } = 0;
    public Task ExecuteAsync()
    {
        if (!Directory.Exists(RuntimeEnvironment.BaseDir))
            Directory.CreateDirectory(RuntimeEnvironment.BaseDir);
        Environment.CurrentDirectory = RuntimeEnvironment.BaseDir;
        
        ResourceDirHandler.CreateResourceDirInfo();
        
        return Task.CompletedTask;
    }
}