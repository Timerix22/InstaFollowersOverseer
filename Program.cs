global using System;
global using System.Threading.Tasks;
global using System.Linq;
global using System.Collections.Generic;
global using DTLib.Filesystem;
global using DTLib.Extensions;
global using DTLib.Dtsod;
global using DTLib.Logging.New;
global using File = DTLib.Filesystem.File;
global using Directory = DTLib.Filesystem.Directory;
global using Path = DTLib.Filesystem.Path;
global using static InstaFollowersOverseer.SharedData;
using System.Net.Http;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InstaFollowersOverseer;

static class Program
{
    static void Main()
    {
        Console.InputEncoding=Encoding.UTF8;
        Console.OutputEncoding=Encoding.UTF8;
        DTLibInternalLogging.SetLogger(MainLogger.ParentLogger);
        try
        {
            config = Config.ReadFromFile();
            userSettings = UserSettings.ReadFromFile();
            
            CancellationTokenSource mainCancel = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                mainCancel.Cancel();
                Thread.Sleep(1000);
                MainLogger.LogInfo("all have cancelled");
                e.Cancel = false;
            };

            var bot = new TelegramBotClient(config.botToken, new HttpClient());
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types

            };
            bot.StartReceiving(BotApiUpdateHandler, BotApiExceptionHandler, receiverOptions, mainCancel.Token);

            Task.Delay(-1, mainCancel.Token).GetAwaiter().GetResult();
            Thread.Sleep(1000);
        }
        catch (Exception ex)
        {
            MainLogger.LogError(ex);
        }
        Console.ResetColor();
    }

    private static ContextLogger botLogger = new ContextLogger("bot", MainLogger.ParentLogger);
    
    static async Task BotApiUpdateHandler(ITelegramBotClient bot, Update update, CancellationToken cls)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    var message = update.Message!;
                    if (message.Text!.StartsWith('/'))
                    {
                        botLogger.LogInfo($"user {message.Chat.Id} sent command {message.Text}");
                        var spl = message.Text.SplitToList(' ');
                        string command = spl[0].Substring(1);
                        spl.RemoveAt(0);
                        string[] args = spl.ToArray();
                        switch (command)
                        {
                            case "start":
                                await bot.SendTextMessageAsync(message.Chat, "hi");
                                break;
                            case "oversee":
                                break;
                            // default:
                            // throw new BotCommandException(command, args);
                        }
                    }
                    else botLogger.LogDebug($"message recieved: {message.Text}");

                    break;
                } /*
            case UpdateType.EditedMessage:
                break;
            case UpdateType.InlineQuery:
                break;
            case UpdateType.ChosenInlineResult:
                break;
            case UpdateType.CallbackQuery:
                break;*/
                default:
                    botLogger.LogWarn($"unknown update type: {update.Type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            botLogger.LogWarn("UpdateHandler", ex);
        }
    }
    static Task BotApiExceptionHandler(ITelegramBotClient bot, Exception ex, CancellationToken cls)
    {
        botLogger.LogError(ex);
        return Task.CompletedTask;
    }
}