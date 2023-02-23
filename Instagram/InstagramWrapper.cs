using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;

namespace InstaFollowersOverseer.Instagram;

public static class InstagramWrapper
{
    public static ContextLogger InstagramLogger = new("instagram",ParentLogger);
    private static IInstaApi Api=null!;
    
    public static async void Init()
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
                .SetRequestDelay(RequestDelay.FromSeconds(0, 1))
                .Build();
            InstagramLogger.LogInfo("instagram login starting");
            var rezult= await Api.LoginAsync();
            if (!rezult.Succeeded)
                throw new Exception("login exception:\n" + rezult.Info + '\n' + rezult.Value);
            InstagramLogger.LogInfo("instagram wrapper have initialized and connected successfully");
        }
        catch (OperationCanceledException) {}
        catch (Exception ex)
        {
            InstagramLogger.LogError("init", ex);
            Program.Stop();
        }
    }

    public static async Task<InstaUser?> GetUserAsync(string usernameOrUrl)
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
}