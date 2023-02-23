using System.Net.Http;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using InstaFollowersOverseer.Instagram;

namespace InstaFollowersOverseer.Telegram;

public static class TelegramWrapper
{
    private static ContextLogger TelegramLogger = new("telegram", ParentLogger);
    private static TelegramBotClient Bot=null!;

    public static async void Init()
    {
        try
        {
            TelegramLogger.LogInfo("initializing telegram wrapper");
            if (CurrentConfig is null)
                throw new NullReferenceException("config is null");
            Bot = new TelegramBotClient(CurrentConfig.botToken, new HttpClient());
            await Bot.SetMyCommandsAsync(new BotCommand[]
            {
                new() { Command = "start", Description = "starts the bot"},
               // new() { Command = "help", Description = "shows commands list" },
                new() { Command = "oversee", Description = "[instagram username] - " + 
                                "enables notifications about instagram user's followers" },
                new() { Command = "list", Description = "shows list of overseeing instagram users" }
            });
            var receiverOptions = new ReceiverOptions
            {
                // AllowedUpdates = { }, // receive all update types
            };
            TelegramLogger.LogInfo("bot starting recieving long polls");
            Bot.StartReceiving(BotApiUpdateHandler, BotApiExceptionHandler, receiverOptions, Program.MainCancelToken);
            TelegramLogger.LogInfo("telegram wrapper have initialized successfully");
        }
        catch (OperationCanceledException) {}
        catch (Exception ex)
        {
            TelegramLogger.LogError("init", ex);
            Program.Stop();
        }
    }

    private static Task BotApiExceptionHandler(ITelegramBotClient bot, Exception ex, CancellationToken cls)
    {
        TelegramLogger.LogError(ex);
        return Task.CompletedTask;
    }

    static async Task SendInfoReply(string text, Message replyToMessage)
    {
        TelegramLogger.LogInfo(text);
        await Bot.SendTextMessageAsync(replyToMessage.Chat, text, 
            replyToMessageId: replyToMessage.MessageId, 
            parseMode:ParseMode.MarkdownV2);
    }
    static async Task SendErrorReply(string text, Message replyToMessage)
    {
        TelegramLogger.LogWarn(text);
        await Bot.SendTextMessageAsync(replyToMessage.Chat, "error: "+text, 
            replyToMessageId: replyToMessage.MessageId, 
            parseMode:ParseMode.MarkdownV2);
    }
    
    private static async Task BotApiUpdateHandler(ITelegramBotClient bot, Update update, CancellationToken cls)
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
                        TelegramLogger.LogInfo($"user {message.Chat.Id} sent command {message.Text}");
                        var spl = message.Text.SplitToList(' ');
                        string command = spl[0].Substring(1);
                        spl.RemoveAt(0);
                        string[] args = spl.ToArray();
                        await ExecCommandAsync(command, args, message);
                    }
                    else TelegramLogger.LogDebug($"message recieved: {message.Text}");
                    break;
                } 
            /*case UpdateType.InlineQuery:
                break;
            case UpdateType.ChosenInlineResult:
                break;
            case UpdateType.CallbackQuery:
                break;*/
                default:
                    TelegramLogger.LogWarn($"unknown update type: {update.Type}");
                    break;
            }
        }
        catch (OperationCanceledException) {}
        catch (Exception ex)
        {
            TelegramLogger.LogWarn("UpdateHandler", ex);
        }
    }

    private static async Task ExecCommandAsync(string command, string[] args, Message message)
    {
        switch (command)
        {
            case "start":
                await Bot.SendTextMessageAsync(message.Chat, "hi");
                break;
            case "oversee":
            {
                string usernameOrUrl = args[0];
                await SendInfoReply($"searching for instagram user <{usernameOrUrl}>", message);
                var user = await InstagramWrapper.GetUserAsync(usernameOrUrl);
                if (user is null)
                {
                    await SendErrorReply($"user **{usernameOrUrl}** doesnt exist", message);
                    return;
                }
                CurrentUsersData.AddOrSet(message.Chat.Id.ToString(), new InstagramObservableParams(usernameOrUrl));
                CurrentUsersData.SaveToFile();
                break;
            }
            default:
                await SendErrorReply("ivalid command", message);
                break;
        }
    }
}