namespace InstaFollowersOverseer;

public class InstagramObservableParams
{
    public string instagramUserId;
    public bool notifyOnFollowing=true;
    public bool notifyOnUnfollowing=true;

    public InstagramObservableParams(string instaUserId)
    {
        instagramUserId = instaUserId;
    }

    public InstagramObservableParams(DtsodV23 _overseeParams)
    {
        instagramUserId = _overseeParams["instagramUserId"];
        if (_overseeParams.TryGetValue("notifyOnFollowing", out var _notifyOnFollowing))
            notifyOnFollowing = _notifyOnFollowing;
        if (_overseeParams.TryGetValue("notifyOnUnfollowing", out var _notifyOnUnfollowing))
            notifyOnUnfollowing = _notifyOnUnfollowing;
    }

    public DtsodV23 ToDtsod()
    {
        var d = new DtsodV23();
        d.Add(nameof(instagramUserId), instagramUserId);
        if(!notifyOnFollowing)
            d.Add(nameof(notifyOnFollowing), false);
        if(!notifyOnUnfollowing)
            d.Add(nameof(notifyOnFollowing), false);
        return d;
    }
    
    public override string ToString() => ToDtsod().ToString();
}