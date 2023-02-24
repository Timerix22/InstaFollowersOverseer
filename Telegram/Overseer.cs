using InstaFollowersOverseer.Instagram;

namespace InstaFollowersOverseer;

public static class Overseer
{
    private static ContextLogger ObserverLogger = new("observer",ParentLogger);

    private static CancellationTokenSource OverseeCancel = new();

    public static async void Start()
    {
        try
        {
            ObserverLogger.LogInfo("observer is starting");
            while (!OverseeCancel.Token.IsCancellationRequested)
            {
                ObserverLogger.LogDebug("loop begins");
                // parallel diff computation per telegram user
                Parallel.ForEach(CurrentUsersData.UsersDict,
                     tgUserData => 
                {
                    try
                    {
                        var instaUsers = tgUserData.Value;
                        long chatId = tgUserData.Key.ToLong();
                        
                        // parallel diff computation per instagram user
                        Parallel.For(0, instaUsers.Count, DiffInstaUser);
                        
                        async void DiffInstaUser(int i)
                        {
                            try
                            {
                                HtmlMessageBuilder b = new();
                                ObserverLogger.LogInfo($"comparing followers lists of user {instaUsers[i]}");
                                // slow operation
                                FollowersDiff diff =
                                    await InstagramWrapper.GetFollowersDiffAsync(instaUsers[i].instagramUsername);
                                
                                b.BeginStyle(TextStyle.Bold | TextStyle.Underline)
                                    .Text(instaUsers[i].instagramUsername)
                                    .EndStyle()
                                    .Text('\n');
                                diff.AppendDiffMessageTo(b, OverseeCancel.Token);
                                ObserverLogger.LogInfo($"sending notification to {tgUserData.Key}");
                                await TelegramWrapper.SendInfo(chatId, b);
                            }
                            catch(OperationCanceledException){}
                            catch (Exception ex)
                            {
                                ObserverLogger.LogWarn("ObserveLoop", ex);
                            }
                        }

                    }
                    catch (OperationCanceledException) {}
                    catch (Exception ex)
                    {
                        ObserverLogger.LogWarn("ObserveLoop", ex);
                    }
                });

                ObserverLogger.LogDebug("loop ends");
                await Task.Delay(TimeSpan.FromMinutes(CurrentConfig.checksIntervalMinutes));
            }
        }
        catch (OperationCanceledException) {}
        catch (Exception ex)
        {
            ObserverLogger.LogError("ObserveLoop", ex);
        }
    }

    public static void Stop()
    {
        ObserverLogger.LogInfo("observer is stopping");
        OverseeCancel.Cancel();
        OverseeCancel = new CancellationTokenSource();
    }
}