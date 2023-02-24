using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using InstaFollowersOverseer.Instagram;

namespace InstaFollowersOverseer;

public static class TelegramWrapper
{
    private static ContextLogger TelegramLogger = new("telegram", ParentLogger);
    private static TelegramBotClient Bot=null!;

    public static async Task InitAsync()
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
            TelegramLogger.LogInfo("telegram wrapper initialized successfully");
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
    
    /// parses text from markdown to html and sends to telegram chat
    public static async Task SendMessage(ChatId chatId, HtmlMessageBuilder message, int? replyToMesId=null)
    {
        string html = message.ToHtml();
        await Bot.SendTextMessageAsync(chatId, html,
            replyToMessageId: replyToMesId,
            parseMode: ParseMode.Html);
        message.Clear();
    }

    public static async Task SendInfo(ChatId chatId, HtmlMessageBuilder message, int? replyToMesId=null)
    {
        TelegramLogger.LogInfo(message);
        await SendMessage(chatId, message, replyToMesId);
    }
    
    public static async Task SendError(ChatId chatId, HtmlMessageBuilder message, int? replyToMesId=null)
    {
        TelegramLogger.LogWarn(message);
        await SendMessage(chatId, new HtmlMessageBuilder().BeginStyle(TextStyle.Bold | TextStyle.Italic)
            .Text("error: ").EndStyle().Text(message), replyToMesId);
    }
    public static async Task SendError(ChatId chatId, Exception ex, int? replyToMesId=null)
    {
        TelegramLogger.LogWarn(ex);
        await SendMessage(chatId, new HtmlMessageBuilder().BeginStyle(TextStyle.Bold | TextStyle.Italic)
            .Text("error: ").EndStyle().Text(ex.Message), replyToMesId);
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
        try
        {
            HtmlMessageBuilder rb = new();
            long senderId = message.From?.Id ?? message.Chat.Id;
            string senderName = message.From?.FirstName ?? message.Chat.FirstName ??
                message.Chat.Username ?? "UnknownUser";
            switch (command)
            {
                case "start":
                    await SendInfo(message.Chat, rb.Text("bot started"));
                    break;
                case "oversee":
                {
                    string usernameOrUrl = args[0];
                    await SendInfo(message.Chat, rb.Text("searching for instagram user"), message.MessageId);
                    var user = await InstagramWrapper.TryGetUserAsync(usernameOrUrl);
                    if (user is null)
                    {
                        await SendError(message.Chat, rb.Text("user ").Text(usernameOrUrl).Text(" not found"));
                        return;
                    }
                    await SendInfo(message.Chat, rb.Text("user ").Text(usernameOrUrl).Text(" found"));
                    // user id or chat id
                    CurrentUsersData.AddOrSet(senderId, new InstagramObservableParams(usernameOrUrl));
                    CurrentUsersData.SaveToFile();
                    break;
                }
                case "list":
                {
                    var userData = CurrentUsersData.Get(senderId);
                    if (userData is null)
                    {
                        await SendError(message.Chat, rb.Text("no data for user"), message.MessageId);
                        return;
                    }

                    rb.Text(userData.Count).Text("instagram users:\n");
                    foreach (var iuParams in userData)
                    {
                        rb.BeginStyle(TextStyle.Bold).Text(iuParams.instagramUsername).EndStyle().Text(" - ");
                        var iu = await InstagramWrapper.TryGetUserAsync(iuParams.instagramUsername);
                        rb.Text(iu is null ? "user no longer exists" : iu.FullName);
                        rb.SetUrl("https://www.instagram.com/"+iuParams.instagramUsername)
                            .BeginStyle(TextStyle.Link)
                            .Text(iuParams.instagramUsername)
                            .EndStyle().Text('\n');
                    }

                    await SendInfo(message.Chat, rb, message.MessageId);
                    break;
                }
                default:
                    await SendError(message.Chat, rb.Text("ivalid command"), message.MessageId);
                    break;
            }
        }
        catch(OperationCanceledException){}
        catch (Exception ex)
        {
            await SendError(message.Chat, ex);
        }
    }
}