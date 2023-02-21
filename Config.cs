namespace InstaFollowersOverseer;

public class Config
{
    
    private const string config_file="config.dtsod";
    private const string config_example_file="config-example.dtsod";
    
    public string botToken;
    public string instagramLogin;
    public string instagramPassword;

    public Config(DtsodV23 configDtsod)
    {
        botToken = configDtsod[nameof(botToken)];
        instagramLogin = configDtsod[nameof(instagramLogin)];
        instagramPassword = configDtsod[nameof(instagramPassword)];
    }

    public static Config ReadFromFile()
    {
        if (!File.Exists(config_file))
        {
            EmbeddedResources.CopyToFile(
                $"{EmbeddedResourcesPrefix}.{config_example_file}", 
                config_example_file);
            throw new Exception($"File {config_file} doesnt exist. You have create config. See {config_example_file}");
        }

        return new Config(new DtsodV23(File.ReadAllText(config_file)));
    }

    public DtsodV23 ToDtsod()
    {
        var d = new DtsodV23
        {
            { nameof(botToken), botToken },
            { nameof(instagramLogin), instagramLogin },
            { nameof(instagramLogin), instagramLogin }
        };
        return d;
    }
    
    public override string ToString() => ToDtsod().ToString();
    
    public void SaveToFile()
    {
        File.Copy(config_file, 
            $"backups/{config_file}.old-"+
            "{DateTime.Now.ToString(MyTimeFormat.ForFileNames)}", 
            true);
        
        File.OpenWrite(config_file)
            .FluentWriteString("#DtsodV23\n")
            .WriteString(ToDtsod().ToString());
    }
}