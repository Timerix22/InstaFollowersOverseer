global using System;
global using System.Threading.Tasks;
global using System.Linq;
global using System.Collections.Generic;
global using DTLib;
global using DTLib.Filesystem;
global using DTLib.Extensions;
global using DTLib.Dtsod;
global using DTLib.Logging.New;
global using File = DTLib.Filesystem.File;
global using Directory = DTLib.Filesystem.Directory;
global using Path = DTLib.Filesystem.Path;
global using static InstaFollowersOverseer.SharedData;
using System.Text;
using System.Threading;

namespace InstaFollowersOverseer;

static class Program
{
    public static readonly ContextLogger MainLogger = new("main", ParentLogger);
    
    private static CancellationTokenSource MainCancel=new();
    public static CancellationToken MainCancelToken = MainCancel.Token;
    public static void Stop() => MainCancel.Cancel();

    
    static void Main()
    {
        Console.InputEncoding=Encoding.UTF8;
        Console.OutputEncoding=Encoding.UTF8;
        DTLibInternalLogging.SetLogger(MainLogger.ParentLogger);
        try
        {
            MainLogger.LogInfo("reading config");
            CurrentConfig.LoadFromFile();
            CurrentUsersData.LoadFromFile();
            
            Console.CancelKeyPress += (_, e) =>
            {
                Stop();
                Thread.Sleep(1000);
                MainLogger.LogInfo("all have cancelled");
                e.Cancel = false;
            };

            Instagram.InstagramWrapper.Init();
            Telegram.TelegramWrapper.Init();

            Task.Delay(-1, MainCancel.Token).GetAwaiter().GetResult();
            Thread.Sleep(1000);
        }
        catch (OperationCanceledException) {}
        catch (Exception ex)
        {
            MainLogger.LogError(ex);
        }
        CurrentConfig.SaveToFile();
        CurrentUsersData.SaveToFile();
        Console.ResetColor();
    }
}