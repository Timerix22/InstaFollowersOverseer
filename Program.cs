global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;
global using DTLib;
global using DTLib.Filesystem;
global using DTLib.Extensions;
global using DTLib.Dtsod;
global using DTLib.Logging.New;
global using File = DTLib.Filesystem.File;
global using Directory = DTLib.Filesystem.Directory;
global using Path = DTLib.Filesystem.Path;
global using static InstaFollowersOverseer.SharedData;

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
                Overseer.Stop();
                e.Cancel = false;
            };

            Task[] tasks={
                Instagram.InstagramWrapper.InitAsync(),
                TelegramWrapper.InitAsync()
            };
            Task.WaitAll(tasks);
            
            Overseer.Start();

            Task.Delay(-1, MainCancel.Token).GetAwaiter().GetResult();
            Thread.Sleep(1000);
            MainLogger.LogInfo("all have cancelled");
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