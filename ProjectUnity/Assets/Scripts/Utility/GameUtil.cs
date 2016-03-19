

using BestHTTP;
using SLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[CustomLuaClass]
public class GameUtil
{
    public static string MakePathForWWW(string path)
    {
        if (path.IndexOf("://") == -1)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            return "file:///" + path;
#else
            return "file://" + path;
#endif
        }
        else
            return path;
    }

    public static string MakePathForLua(string name)
    {
        string path = LuaPath;
        string lowerName = name.ToLower();
        if (lowerName.EndsWith(".lua"))
        {
            int index = name.LastIndexOf('.');
            name = name.Substring(0, index);
        }
        name = name.Replace('.', '/');
        path = path + name + ".lua";
        return path;
    }

    public static string GetPlatformFolderForAssetBundles()
    {
#if UNITY_IOS
			return "iOS";
#elif UNITY_ANDROID
        return "Android";
#elif UNITY_WEBPLAYER
            return "WebPlayer";
#elif UNITY_WP8
			return "WP8Player";
#elif UNITY_METRO
            return "MetroPlayer";
#elif UNITY_OSX || UNITY_STANDALONE_OSX
		return "OSX";
#elif UNITY_STANDALONE_WIN
        return "Windows";
#else
        return "";
#endif
    }

    private static string assetRoot;
    public static string AssetRoot
    {
        get
        {
            if (!string.IsNullOrEmpty(assetRoot))
                return assetRoot;
#if UNITY_EDITOR
            return Application.dataPath + "/../../Output/" + AppConst.AssetDirname + "/" ;
#else
            return Application.persistentDataPath + "/" + AppConst.AssetDirname + "/";
#endif
        }
        set
        {
            assetRoot = value;
        }
    }

    public static string AssetPath
    {
        get
        {
            string platform = GetPlatformFolderForAssetBundles();
            if (platform != "")
                return AssetRoot + platform + "/" + AppConst.AssetDirname + "/";
            else
                return AssetRoot + AppConst.AssetDirname + "/";
        }
    }

    private static string luaPath;
    public static string LuaPath
    {
        get
        {
            if (!string.IsNullOrEmpty(luaPath))
                return luaPath;
            return AssetRoot + "Lua/";
        }
        set
        {
            luaPath = value;
        }
    }

    public static string DataPath
    {
        get
        {
#if UNITY_EDITOR
            return Application.dataPath + "/";
#else
            return Application.persistentDataPath + "/";
#endif
        }
    }

    public static string CachePath
    {
        get
        {
#if UNITY_EDITOR
            return Application.dataPath + "/../Cache/";
#else
            return Application.temporaryCachePath + "/";
#endif
        }
    }

    public static string SepPath
    {
        get
        {
            return DataPath + "pck/";
        }
    }

    public static bool IsSepFileExist(string f, out string filename)
    {
        string fn = f.ToLower();
        if (File.Exists(SepPath + fn))
        {
            filename = SepPath + fn;
            return true;
        }
        else
        {
            filename = null;
            return false;
        }
    }

    public static void CreateDirectory(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
    public static void CreateDirectoryForFile(string filepath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));
    }
    public static bool IsDirectoryExist(string dir)
    {
        return Directory.Exists(dir);
    }

    public static string RapFilePath(string fullname)
    {
        return fullname.Replace('\\', '/');
    }

    public static string GetFilePath(string fullname)
    {
        String temp = RapFilePath(fullname);

        int nPos = temp.LastIndexOf('/');
        if (nPos != -1)
        {
            return temp.Substring(0, nPos);
        }
        else
            return temp;
    }
    public static string FileName(string fullname)
    {
        String temp = RapFilePath(fullname);

        int nPos = temp.LastIndexOf('/');
        if (nPos != -1)
        {
            return temp.Substring(nPos + 1);
        }
        else
            return temp;
    }
    public static string FileNameWithoutExt(string fullname)
    {
        int nPos = fullname.LastIndexOf('.');
        if (nPos != -1)
        {
            return fullname.Substring(0, nPos);
        }
        else
            return fullname;
    }
    static bool ContainsFileExt(string[] filters, string ext)
    {
        for (int i = 0; i < filters.Length; ++i)
        {
            if (filters[i].Equals(ext))
                return true;
        }
        return false;
    }

    [DoNotToLua]
    public static void Recursive(ref List<string> files, string path, string[] filters)
    {
        try
        {
            string[] names = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            foreach (string filename in names)
            {
                string ext = Path.GetExtension(filename);
                if (filters != null && ContainsFileExt(filters, ext)) continue;
                files.Add(filename.Replace('\\', '/'));
            }
            foreach (string dir in dirs)
            {
                Recursive(ref files, dir, filters);
            }
        }
        catch (Exception e)
        {
            LogUtil.LogWarning(e.Message);
        }
    }

    public static List<string> RecursiveFiles(string path, string[] filters)
    {
        List<string> ret = new List<string>();
        Recursive(ref ret, path, filters);
        return ret;
    }

    public static string ToHexString(byte[] bytes, string sep = ",")
    {
        string byteStr = string.Empty;
        if (bytes != null || bytes.Length > 0)
        {
            int nPos = 0;
            foreach (var item in bytes)
            {
                nPos++;
                byteStr += string.Format("{0:X2}", item);
                if (nPos < bytes.Length)
                    byteStr += sep;
            }
        }
        return byteStr;
    }

    public static string ToBytesString(byte[] bytes, string sep = ",")
    {
        string byteStr = string.Empty;
        if (bytes != null || bytes.Length > 0)
        {
            int nPos = 0;
            foreach (var item in bytes)
            {
                nPos++;
                byteStr += item.ToString();
                if (nPos < bytes.Length)
                    byteStr += sep;
            }
        }
        return byteStr;
    }

    static IEnumerator _ansy_open_file_(string path, System.Action<bool, object> cb)
    {
        WWW www = new WWW(path);
        yield return www;
        if (www.isDone)
        {
            cb(true, www);
        }
        else
        {
            cb(false, www.error);
        }
    }

    public static void AnsyOpenFile(string filePath, LuaFunction cb)
    {
        string filename = MakePathForWWW(filePath);
        EntryPoint.Instance.StartCoroutine(_ansy_open_file_(filename, (success, o) =>
        {
            cb.call(success, o);
            cb.Dispose();
        }));
    }

    //异步HTTP
    public static void SendRequest(string url, string data,double t,bool bGet, LuaFunction completeHandler)
    {
        //如果web页面是静态返回数据，请用HTTPMethods.Get
        var request = new HTTPRequest(new Uri(url), bGet ? HTTPMethods.Get : HTTPMethods.Post, (req, resp) =>
        {
            if (completeHandler != null)
            {
                completeHandler.call(req, resp);  //req, resp 需要暴露给slua导出
                completeHandler.Dispose();
            }
        });
        request.RawData = Encoding.UTF8.GetBytes(data);
        request.ConnectTimeout = TimeSpan.FromSeconds(t);//超时
        request.Send();
    }

    //异步下载，参数  complete_param 是完成回调的执行参数
    public static void DownLoad(string SrcFilePath, string SaveFilePath, bool bGet, bool keepAlive,object complete_param, LuaFunction progressHander, LuaFunction completeHander)
    {
        
        var request = new HTTPRequest(new Uri(SrcFilePath), bGet ? HTTPMethods.Get : HTTPMethods.Post, keepAlive, (req, resp) =>
        {
            List<byte[]> fragments = null;
            string status = "";
            switch (req.State)
            {
                case HTTPRequestStates.Processing:
                    {
                        fragments = resp.GetStreamedFragments();
                        if (fragments != null && fragments.Count > 0)
                        {
                            FileStream fs = new FileStream(SaveFilePath, FileMode.Append);
                            foreach (byte[] data in fragments)
                                fs.Write(data, 0, data.Length);
                        }
                    }
                    break;
                case HTTPRequestStates.Finished:
                    {
                        if (resp.IsSuccess)
                        {
                            // Save any remaining fragments
                            fragments = resp.GetStreamedFragments();
                            if (fragments != null && fragments.Count > 0)
                            {
                                FileStream fs = new FileStream(SaveFilePath, FileMode.Append);
                                foreach (byte[] data in fragments)
                                    fs.Write(data, 0, data.Length);
                            }
                            if(resp.IsStreamingFinished)
                            {
                                status = "Streaming finished!";
                                if (completeHander != null)
                                {
                                    completeHander.call(true, req, resp, complete_param);
                                    completeHander.Dispose();
                                }
                                else
                                    LogUtil.Log(status);
                            }
                        }
                        else
                        {
                            status = string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                            resp.StatusCode,
                                                            resp.Message,
                                                            resp.DataAsText);
                            if (completeHander != null)
                            {
                                completeHander.call(false, req, resp, status);
                                completeHander.Dispose();
                            }
                            else
                                LogUtil.LogWarning(status);
                        }
                    }
                    break;
                case HTTPRequestStates.Error:
                    {
                        status = "Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                        if (completeHander != null)
                        {
                            completeHander.call(false, req, resp, status);
                            completeHander.Dispose();
                        }
                        else
                            LogUtil.LogWarning(status);
                    }
                    break;
                case HTTPRequestStates.Aborted:
                    {
                        status = "Request Aborted!";
                        if (completeHander != null)
                        {
                            completeHander.call(false, req, resp, status);
                            completeHander.Dispose();
                        }
                        else
                            LogUtil.LogWarning(status);

                    }
                    break;
                case HTTPRequestStates.ConnectionTimedOut:
                    {
                        status = "Connection Timed Out!";
                        if (completeHander != null)
                        {
                            completeHander.call(false, req, resp, status);
                            completeHander.Dispose();
                        }
                        else
                            LogUtil.LogWarning(status);
                    }
                    break;
                case HTTPRequestStates.TimedOut:
                    {
                        status = "Processing the request Timed Out!";
                        if (completeHander != null)
                        {
                            completeHander.call(false, req, resp, status);
                            completeHander.Dispose();
                        }
                        else
                            LogUtil.LogWarning(status);
                    }
                    break;

            }
        });
        request.OnProgress = (req, downloaded, length) =>
        {
            if (progressHander != null)
            {
                double pg = Math.Round((float)downloaded / (float)length, 2);
                progressHander.call(pg, downloaded,length);
                progressHander.Dispose();
            }
        };
        request.UseStreaming = true;
        request.StreamFragmentSize = 1 * 1024 * 1024; // 1 megabyte
        request.DisableCache = true; // already saving to a file, so turn off caching
        request.Send();
    }

    
    public static void ReStart(LuaFunction cb)
    {
        EntryPoint.Instance.ReStart(() =>
        {
            cb.call();
            cb.Dispose();
        });
    }

    static IEnumerator _ansy_loadlevel(AsyncOperation asr, System.Action<bool> cb)
    {
        yield return asr;
        if (asr.isDone)
        {
            cb(true);
        }
        else
        {
            cb(false);
        }
    }

    public static void AnsyLoadLevel(string levelName,LuaFunction cb)
    {
        AsyncOperation asr = Application.LoadLevelAsync(levelName);
        EntryPoint.Instance.StartCoroutine(_ansy_loadlevel(asr, (success) =>
        {
            cb.call(success);
            cb.Dispose();
        }));
    }
}