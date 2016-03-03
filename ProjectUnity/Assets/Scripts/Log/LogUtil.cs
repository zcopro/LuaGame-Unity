using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class LogUtil
{
    public enum LogLevel
    {
        None,
        Info,
        Warn,
        Error,
        Exeception,
        Abort,
    }

    public static LogLevel loglevel = LogLevel.Info;

    public static void Log(string str, params object[] args)
    {
        str = string.Format(str, args);
        Debug.Log(str);
    }

    public static void LogWarning(string str, params object[] args)
    {
        str = string.Format(str, args);
        Debug.LogWarning(str);
    }

    public static void LogError(string str, params object[] args)
    {
        str = string.Format(str, args);
        Debug.LogError(str);
    }

    public static void LogException(System.Exception ex, Object context = null)
    {
        if (null == context)
            Debug.LogException(ex);
        else
            Debug.LogException(ex, context);
    }

    class LogMessage
    {
        public LogType type;
        public string str;
    }
    static List<LogMessage> sCachedLogList = new List<LogMessage>();

    static void LogCallback(string condition, string stackTrace, LogType type)
    {
        if(condition.Contains("FireEvent:UNITY_LOG"))
        {
            type = LogType.Error;
        }
        if(type == LogType.Log)
        {
            ProcessLog(type, condition);
        }  
        else if(type == LogType.Warning)
        {
            ProcessLog(type, condition);
        }
        else
        {
            ProcessLog(type, condition);
            ProcessLog(type, stackTrace);
        }
    }

    static void ProcessLog(LogType type,string str)
    {
        if (sCachedLogList.Count > 0)
        {
            foreach (var log in sCachedLogList)
            {
                if (!ReportLog(log.type, log.str))
                {
                    CacheLog(type, str);
                    return;
                }
            }
            sCachedLogList.Clear();
        }

        if (!ReportLog(type,str))
        {
            CacheLog(type, str);
        }
        else
        {
           sCachedLogList.Clear();
        }
    }

    static bool ReportLog(LogType type,string str)
    {
        if (null == FLua.LuaSvr.mainLuaState)
            return false;
        FLua.LuaState l = FLua.LuaSvr.mainLuaState.luaState;
        if (null == l)
            return false;
        FLua.LuaFunction func = l.getFunction("OnUnityLog");
        if (null != func)
        {
            func.call(new object[] { type, str });
            func.Dispose();
            return true;
        }
        return false;
    }
    static void CacheLog(LogType type,string str)
    {
        sCachedLogList.Add(new LogMessage{ type = type, str = str });
    }

    public static void AttachUnityLogHandle()
    {
        Application.logMessageReceived += LogCallback;
    }
    public static void DetachUnityLogHandle()
    {
        Application.logMessageReceived -= LogCallback;
    }

}
