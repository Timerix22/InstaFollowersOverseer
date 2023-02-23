using System.IO;

namespace InstaFollowersOverseer;

public abstract class DtsodFile
{
    public readonly string FileNameWithoutExt;
    public readonly string FileName;
    public readonly string FileExampleName;

    public DtsodFile(string fileNameWithoutExt)
    {
        FileNameWithoutExt = fileNameWithoutExt;
        FileName = fileNameWithoutExt + ".dtsod";
        FileExampleName = fileNameWithoutExt + "-example.dtsod";
    }
    
    public void CreateBackup()
    {
        string backupPath=$"backups/{FileNameWithoutExt}.d/{FileNameWithoutExt}"
                          +DateTime.Now.ToString(MyTimeFormat.ForFileNames)+".dtsod";
        Program.MainLogger.LogInfo($"creating backup if file {FileName} at path {backupPath}");
        File.Copy(FileName,backupPath,false);
    }
    
    public DtsodV23 ReadDtsodFromFile(bool trhowIfFileNotFound)
    {
        Program.MainLogger.LogInfo($"reading file {FileName}");
        EmbeddedResources.CopyToFile(
            $"{EmbeddedResourcesPrefix}.{FileExampleName}",
            FileExampleName);

        if (!File.Exists(FileName))
        {
            File.WriteAllText(FileName, "#DtsodV23\n");
            string message = $"file {FileName} doesnt exist, created new blank";
            if (trhowIfFileNotFound) 
                throw new FileNotFoundException(message);
            Program.MainLogger.LogWarn(message);
            return new DtsodV23();
        }

        string fileText = File.ReadAllText(FileName);
        Program.MainLogger.LogDebug(fileText);
        return new DtsodV23(fileText);
    }

    public abstract void LoadFromFile();
    
    public abstract DtsodV23 ToDtsod();
    
    public void SaveToFile()
    {
        Program.MainLogger.LogInfo($"saving file {FileName}");
        string dtsodStr = ToDtsod().ToString();
        Program.MainLogger.LogDebug(dtsodStr);
        if(File.Exists(FileName))
            CreateBackup();
        File.OpenWrite(FileName)
            .FluentWriteString("#DtsodV23\n")
            .FluentWriteString(dtsodStr)
            .Close();
    }
}