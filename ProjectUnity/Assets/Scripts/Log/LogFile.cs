using UnityEngine;
using System.Collections;
using System.IO;

public class LogFile {

    const string LogFileName = "gamelog.log";
    const string ErrorFlag = "has_error";

    private static LogFile s_instance = null;
    public static LogFile Instance {
        get
        {
            if (s_instance == null)
                s_instance = new LogFile();

            return s_instance;
        }
    }

    string LogPath
    {
        get { return GameUtil.CachePath + "logs/"; }
    }
    string BackupPath
    {
        get { return GameUtil.CachePath + "logs/backup/"; }
    }

    public void Backup()
    {
        if(File.Exists(LogPath + LogFileName))
        {
            GameUtil.CreateDirectoryForFile(BackupPath + LogFileName);
            File.Copy(LogPath + LogFileName, BackupPath + LogFileName, true);
        }
    }

    public void Init()
    {
        Backup();
        Application.logMessageReceived += LogCallback;
        if (File.Exists(LogPath + LogFileName))
            File.Delete(LogPath + LogFileName);
        if (File.Exists(LogPath + ErrorFlag))
            File.Delete(LogPath + ErrorFlag);

        if (File.Exists(BackupPath + LogFileName))
            File.Delete(BackupPath + LogFileName);
        if (File.Exists(BackupPath + ErrorFlag))
            File.Delete(BackupPath + ErrorFlag);

        GameUtil.CreateDirectoryForFile(LogPath + LogFileName);
        GameUtil.CreateDirectoryForFile(BackupPath + LogFileName);
        
        using (File.Create(LogPath + LogFileName))
        {

        }
        LogUtil.Log("LogPath:" + LogPath);
    }

    public void UnInit()
    {
        Application.logMessageReceived -= LogCallback;
    }

    void LogCallback(string condition, string stackTrace, LogType type)
    {
        switch(type)
        {
            case LogType.Error:
                if (!File.Exists(LogPath + LogFileName))
                {
                    using (StreamWriter sw = File.CreateText(LogPath + LogFileName))
                    {
                        sw.WriteLine(string.Format("{0}:{1}",condition, stackTrace));
                        sw.Flush();
                    }
                }
                WriteToFile(string.Format("[error]{0}:{1}",condition, stackTrace));
                break;
            case LogType.Exception:
                if (!File.Exists(LogPath + LogFileName))
                {
                    using (StreamWriter sw = File.CreateText(LogPath + LogFileName))
                    {
                        sw.WriteLine(string.Format("{0}:{1}", condition, stackTrace));
                        sw.Flush();
                    }
                }
                WriteToFile(string.Format("[exception]{0}:{1}", condition, stackTrace));
                break;
            case LogType.Assert:
                WriteToFile(string.Format("[assert]{0}", condition));
                break;
            case LogType.Log:
                WriteToFile(string.Format("[info]{0}", condition));
                break;
            case LogType.Warning:
                WriteToFile(string.Format("[warning]{0}", condition));
                break;
        }
    }

    void WriteToFile(string message)
    {
        using (StreamWriter sw = File.AppendText(LogPath + LogFileName))
        {
            sw.WriteLine(message);
            sw.Flush();
        }
    }
}
