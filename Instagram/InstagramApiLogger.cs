using System.Net.Http;
using InstaSharper.Logger;

namespace InstaFollowersOverseer.Instagram;

public class InstagramApiLogger : IInstaLogger
{
    public ContextLogger _logger = new("api", InstagramWrapper.InstagramLogger);
    
    public void LogRequest(HttpRequestMessage r)
    {
        _logger.LogDebug("http",$"request {r.Method.Method.ToUpper()} from {r.RequestUri}:\n"
            + r.Content?.ReadAsStringAsync().GetAwaiter().GetResult());
    }

    public void LogRequest(Uri uri)
    {
        
    }

    public void LogResponse(HttpResponseMessage r)
    {
        _logger.LogDebug("http",$"responce from " +
            (r.RequestMessage!=null && r.RequestMessage.RequestUri!=null ? r.RequestMessage.RequestUri.ToString() : "unknown")
            + $" :\n "+ r.Content.ReadAsStringAsync().GetAwaiter().GetResult());
    }

    public void LogException(Exception ex)
    {
        _logger.LogError(ex);
    }

    public void LogInfo(string info)
    {
        _logger.LogInfo(info);
    }
}