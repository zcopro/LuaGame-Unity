

using SLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CustomLuaClass]
public class GameUtil
{
    public static string MakePathForWWW(string path)
    {
        if (path.IndexOf("//") == -1)
        {
            return "file://" + path;
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
#elif UNITY_OSX
		return "OSX";
#elif UNITY_STANDALONE_OSX
		return  "StandaloneOSXIntel";
#elif UNITY_STANDALONE_WIN
        return "Windows";
#else
        return "";
#endif
    }

    public static string AssetPath
    {
        get
        {
#if UNITY_EDITOR
            string platform = GetPlatformFolderForAssetBundles();
            if (platform != "")
                return Application.dataPath + "/../../Output/" + AppConst.AssetDirname + "/" + platform + "/" + AppConst.AssetDirname + "/" ;
            else
                return Application.dataPath + "/../../Output/" + AppConst.AssetDirname + "/";
#else
            string platform = GetPlatformFolderForAssetBundles();
            if( platform != "")
                return Application.streamingAssetsPath + "/" + platform + "/" + AppConst.AssetDirname + "/";
            else
                return Application.streamingAssetsPath + "/";
#endif
        }
    }

    public static string LuaPath
    {
        get
        {
#if UNITY_EDITOR
            return Application.dataPath + "/../../Output/Lua/";
#else
            string platform = GetPlatformFolderForAssetBundles();
            if( platform != "")
                return Application.streamingAssetsPath + "/" + platform + "/" + AppConst.AssetDirname + "/";
            else
                return Application.streamingAssetsPath + "/";
#endif
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
            return Application.dataPath + "/Cache/";
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
}