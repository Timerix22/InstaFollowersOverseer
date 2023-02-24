namespace InstaFollowersOverseer;

public class Config : DtsodFile
{
    #nullable disable
    public string botToken;
    public string instagramLogin;
    public string instagramPassword;
    public double checksIntervalMinutes;
    #nullable enable

    public Config(string fileNameWithoutExt) : base(fileNameWithoutExt) { }
    
    public override void LoadFromFile()
    {
        var dtsod = ReadDtsodFromFile(true);
        try
        {
            botToken = dtsod[nameof(botToken)];
            instagramLogin = dtsod[nameof(instagramLogin)];
            instagramPassword = dtsod[nameof(instagramPassword)];
            checksIntervalMinutes = dtsod[nameof(checksIntervalMinutes)];
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
        var d = new DtsodV23
        {
            { nameof(botToken), botToken },
            { nameof(instagramLogin), instagramLogin },
            { nameof(instagramPassword), instagramPassword },
            { nameof(checksIntervalMinutes), checksIntervalMinutes }
        };
        return d;
    }
}