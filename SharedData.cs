namespace InstaFollowersOverseer;

public static class SharedData
{
    internal const string EmbeddedResourcesPrefix = "InstaFollowersOverseer.resources";
    
#nullable disable
    internal static Config config;
    internal static UserSettings userSettings;
#nullable enable
    
    public static readonly ContextLogger MainLogger = new ContextLogger("main",new CompositeLogger(
            new ConsoleLogger(),
            new FileLogger("logs","InstaFollowersOverseer"))
        );
}