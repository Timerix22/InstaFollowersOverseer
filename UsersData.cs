namespace InstaFollowersOverseer;

public class UsersData : DtsodFile
{
    private Dictionary<string, List<InstagramObservableParams>> usersData=new();
    
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

                usersData.Add(telegramUserId, oparams);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"your {FileName} format is invalid\n"
                                + $"See {FileExampleName}", innerException: ex);
        }
    }
    
    public override DtsodV23 ToDtsod()
    {
        var b = new DtsodV23();
        foreach (var userS in usersData) 
            b.Add(userS.Key, 
                userS.Value.Select<InstagramObservableParams, DtsodV23>(iop =>
                    iop.ToDtsod()
                ).ToList());
        return b;
    }

    public List<InstagramObservableParams> Get(string telegramUserId)
    {
        if (!usersData.TryGetValue(telegramUserId, out var overseeParams))
            throw new Exception($"there is no settings for user {telegramUserId}");
        return overseeParams;
    }

    public void AddOrSet(string telegramUserId, InstagramObservableParams instagramObservableParams)
    {
        // Add
        // doesnt contain settings for telegramUserId
        if (!usersData.TryGetValue(telegramUserId, out var thisUsersData))
        {
            usersData.Add(telegramUserId, new (){ instagramObservableParams });
            return;
        }

        // Set
        // settings for telegramUserId contain InstagramObservableParams with instagramObservableParams.instagramUserId
        for (var i = 0; i < thisUsersData.Count; i++)
        {
            if (thisUsersData[i].instagramUserId == instagramObservableParams.instagramUserId)
            {
                thisUsersData[i] = instagramObservableParams;
                return;
            }
        }
        
        // Add
        // doesnt contain InstagramObservableParams with instagramObservableParams.instagramUserId
        thisUsersData.Add(instagramObservableParams);
    }

    public void AddOrSet(string telegramUserId, IEnumerable<InstagramObservableParams> instagramObservableParams)
    {
        foreach (var p in instagramObservableParams) 
            AddOrSet(telegramUserId, p);
    }
}