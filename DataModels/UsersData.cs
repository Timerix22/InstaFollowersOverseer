namespace InstaFollowersOverseer;

public class UsersData : DtsodFile
{
    public Dictionary<string, List<InstagramObservableParams>> UsersDict=new();

    public UsersData(string fileName) : base(fileName) {}
    
    public override void LoadFromFile()
    {
        var dtsod=ReadDtsodFromFile(false);
        try
        {
            foreach (var uset in dtsod)
            {
                string telegramUserId = uset.Key;

                List<InstagramObservableParams> oparams = new();
                foreach (DtsodV23 _overseeParams in uset.Value)
                    oparams.Add(new InstagramObservableParams(_overseeParams));

                UsersDict.Add(telegramUserId, oparams);
            }
        }
        catch (Exception ex)
        {
            LoadedSuccessfully = false;
            throw new Exception($"your {FileName} format is invalid\n"
                                + $"See {FileExampleName}", innerException: ex);
        }
    }
    
    public override DtsodV23 ToDtsod()
    {
        var b = new DtsodV23();
        foreach (var userS in UsersDict) 
            b.Add(userS.Key, 
                userS.Value.Select<InstagramObservableParams, DtsodV23>(iop =>
                    iop.ToDtsod()
                ).ToList());
        return b;
    }

    public List<InstagramObservableParams>? Get(long telegramUserId)
    {
        string userIdStr = telegramUserId.ToString();
        if (!UsersDict.TryGetValue(userIdStr, out var overseeParams))
            return null;
        return overseeParams;
    }

    public void AddOrSet(long telegramUserId, InstagramObservableParams instagramObservableParams)
    {
        // Add
        // doesnt contain settings for telegramUserId
        string userIdStr = telegramUserId.ToString();
        if (!UsersDict.TryGetValue(userIdStr, out var thisUsersData))
        {
            UsersDict.Add(userIdStr, new (){ instagramObservableParams });
            return;
        }

        // Set
        // settings for telegramUserId contain InstagramObservableParams with instagramObservableParams.instagramUsername
        for (var i = 0; i < thisUsersData.Count; i++)
        {
            if (thisUsersData[i].instagramUsername == instagramObservableParams.instagramUsername)
            {
                thisUsersData[i] = instagramObservableParams;
                return;
            }
        }
        
        // Add
        // doesnt contain InstagramObservableParams with instagramObservableParams.instagramUsername
        thisUsersData.Add(instagramObservableParams);
    }

    public void AddOrSet(long telegramUserId, IEnumerable<InstagramObservableParams> instagramObservableParams)
    {
        foreach (var p in instagramObservableParams) 
            AddOrSet(telegramUserId, p);
    }
}