namespace InstaFollowersOverseer;

public class UserSettings
{
    private const string user_settings_file="user-settings.dtsod";
    private const string user_settings_example_file="user-settings-example.dtsod";
    
    private Dictionary<string, List<InstagramObservableParams>> userSettings=new();

    private UserSettings()
    {
        
    }
    
    public UserSettings(DtsodV23 _userSettings)
    {
        try
        {
            foreach (var uset in _userSettings)
            {
                string telegramUserId = uset.Key;

                List<InstagramObservableParams> oparams = new List<InstagramObservableParams>();
                foreach (DtsodV23 _overseeParams in uset.Value)
                    oparams.Add(new InstagramObservableParams(_overseeParams));

                userSettings.Add(telegramUserId, oparams);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"your {user_settings_file} format is invalid\n"
                                + $"See {user_settings_example_file}", innerException:ex);
        }
    }

    public static UserSettings ReadFromFile()
    {
        EmbeddedResources.CopyToFile(
            $"{EmbeddedResourcesPrefix}.{user_settings_example_file}",
            user_settings_example_file);
        
        if (File.Exists(user_settings_file))
            return new UserSettings(new DtsodV23(File.ReadAllText(user_settings_file)));

        MainLogger.LogWarn($"file {user_settings_file} doesnt exist, creating new");
        File.WriteAllText(user_settings_file,"#DtsodV23\n");
        return new UserSettings();
    }

    public DtsodV23 ToDtsod()
    {
        var b = new DtsodV23();
        foreach (var userS in userSettings) 
            b.Add(userS.Key, 
                userS.Value.Select(iop =>
                    iop.ToDtsod()
                ).ToList());
        return b;
    }

    public override string ToString() => ToDtsod().ToString();

    public void SaveToFile()
    {
        File.Copy(user_settings_file, 
            $"backups/{user_settings_file}.old-"+
                "{DateTime.Now.ToString(MyTimeFormat.ForFileNames)}", 
            true);
        
        File.OpenWrite(user_settings_file)
            .FluentWriteString("#DtsodV23\n")
            .WriteString(ToDtsod().ToString());
    }
    
    public List<InstagramObservableParams> Get(string telegramUserId)
    {
        if (!userSettings.TryGetValue(telegramUserId, out var overseeParams))
            throw new Exception($"there is no settings for user {telegramUserId}");
        return overseeParams;
    }

    public void AddOrSet(string telegramUserId, InstagramObservableParams instagramObservableParams)
    {
        // Add
        // doesnt contain settings for telegramUserId
        if (!userSettings.TryGetValue(telegramUserId, out var thisUserSettings))
        {
            userSettings.Add(telegramUserId, new (){ instagramObservableParams });
            return;
        }

        // Set
        // settings for telegramUserId contain InstagramObservableParams with instagramObservableParams.instagramUserId
        for (var i = 0; i < thisUserSettings.Count; i++)
        {
            if (thisUserSettings[i].instagramUserId == instagramObservableParams.instagramUserId)
            {
                thisUserSettings[i] = instagramObservableParams;
                return;
            }
        }
        
        // Add
        // doesnt contain InstagramObservableParams with instagramObservableParams.instagramUserId
        thisUserSettings.Add(instagramObservableParams);
    }

    public void AddOrSet(string telegramUserId, IEnumerable<InstagramObservableParams> instagramObservableParams)
    {
        foreach (var p in instagramObservableParams) 
            AddOrSet(telegramUserId, p);
    }
}