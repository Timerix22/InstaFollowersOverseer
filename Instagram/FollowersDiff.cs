using DTLib.Ben.Demystifier.Enumerable;

namespace InstaFollowersOverseer.Instagram;

public readonly record struct FollowersDiff(IList<string> Unfollowed, IList<string> Followed)
{
    public static readonly FollowersDiff Empty =
        new FollowersDiff(EnumerableIList<string>.Empty, EnumerableIList<string>.Empty);

    public bool IsEmpty() => Followed.Count + Unfollowed.Count == 0;
    
    /// <summary>
    /// generates message aouut followed and unfollowed users
    /// </summary>
    /// <param name="b">string builder to append the message to</param>
    /// <param name="ct">diff computation happens in this method because it enumerates yield returned enumerables</param>
    public void AppendDiffMessageTo(HtmlMessageBuilder b, CancellationToken ct)
    {
        if (Followed.Count != 0)
        {
            b.BeginStyle(TextStyle.Italic).Text(Followed.Count).Text(" users followed:\n").EndStyle();
            foreach (var u in Followed)
            {
                if (ct.IsCancellationRequested)
                    return;
                // username with clickable link
                b.SetUrl("https://www.instagram.com/" + u).BeginStyle(TextStyle.Link).Text(u).EndStyle()
                    .Text('\n');
            }
        }

        if (Unfollowed.Count != 0)
        {
            b.BeginStyle(TextStyle.Italic).Text(Unfollowed.Count).Text(" users unfollowed:\n").EndStyle();
            foreach (var u in Followed)
            {
                if (ct.IsCancellationRequested)
                    return;
                // username with clickable link
                b.SetUrl("https://www.instagram.com/" + u).BeginStyle(TextStyle.Link).Text(u).EndStyle()
                    .Text('\n');
            }
        }
    }
}