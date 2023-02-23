namespace InstaFollowersOverseer;

public static class SharedData
{
    internal const string EmbeddedResourcesPrefix = "InstaFollowersOverseer.resources";
    
    internal static Config CurrentConfig = new("config");
    internal static UsersData CurrentUsersData = new("users-data");

    public static readonly CompositeLogger ParentLogger = new(
        new ConsoleLogger(),
        new FileLogger("logs", "InstaFollowersOverseer"));
}