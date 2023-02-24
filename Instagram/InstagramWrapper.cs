using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;

namespace InstaFollowersOverseer.Instagram;

public static class InstagramWrapper
{
    public static ContextLogger InstagramLogger = new("instagram",ParentLogger);
    private static IInstaApi Api=null!;
    
    public static async Task InitAsync()
    {
        try
        {
            InstagramLogger.LogInfo("initializing instagram wrapper");
            if (CurrentConfig is null)
                throw new NullReferenceException("config is null");
            var apiLogger = new InstagramApiLogger();
            // disabling http request/responce logging
            apiLogger._logger.DebugLogEnabled = false;
            Api = InstaApiBuilder.CreateBuilder()
                .UseLogger(apiLogger)
                .SetUser(new UserSessionData
                {
                    UserName = CurrentConfig.instagramLogin,
                    Password = CurrentConfig.instagramPassword
                })
                .SetRequestDelay(RequestDelay.FromSeconds(5, 10))
                .Build();
            InstagramLogger.LogInfo("instagram login starting");
            var rezult= await Api.LoginAsync();
            if (!rezult.Succeeded)
                throw new Exception("login exception:\n" + rezult.Info + '\n' + rezult.Value);
            InstagramLogger.LogInfo("instagram wrapper initialized and connected successfully");
        }
        catch (OperationCanceledException) {}
        catch (Exception ex)
        {
            InstagramLogger.LogError("init", ex);
            Program.Stop();
        }
    }

    public static async Task<InstaUser?> TryGetUserAsync(string usernameOrUrl)
    {
        // url
        if (usernameOrUrl.Contains('/'))
        {
            throw new NotImplementedException("get user by url");
        }
        
        // username
        var u=await Api.GetUserAsync(usernameOrUrl);
        return u.Succeeded ? u.Value : null;
    }

    private static Dictionary<string, IEnumerable<string>> FollowersDict=new();

    /// may took long time if user have many followers
    public static async Task<FollowersDiff> GetFollowersDiffAsync(string instaUser)
    {
        if (await TryGetUserAsync(instaUser) is null)
            throw new Exception($"instagram user {instaUser} doesnt exist");
        var maybeFollowers = await Api.GetUserFollowersAsync(instaUser, PaginationParameters.Empty);
        if (!maybeFollowers.Succeeded)
            throw new Exception($"can't get followers of user {instaUser}");
        var currentFollowers = maybeFollowers.Value.Select(f=>f.UserName).ToList();
        if(!FollowersDict.TryGetValue(instaUser, out var _prevFollowers))
        {
            FollowersDict.Add(instaUser, currentFollowers);
            return FollowersDiff.Empty;
        }
        var prevFollowers = _prevFollowers.ToList();
        var unfollowed = prevFollowers.Except(currentFollowers).ToList();
        var followed = currentFollowers.Except(prevFollowers).ToList();
        return new FollowersDiff(unfollowed, followed);
    }
}