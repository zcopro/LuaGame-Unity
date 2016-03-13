using UnityEngine;
using System.Collections;
using SLua;
using System.IO;
using FGame.Manager;
using System;
using System.Collections.Generic;

public class EntryPoint : MonoBehaviour {
    private static EntryPoint s_instance = null;

    public static EntryPoint Instance
    {
        get { return s_instance; }
    }

    public string EntryLuaScript = string.Empty;
	public LuaSvrFlag SrvFlag = LuaSvrFlag.LSF_BASIC;
    public LogUtil.LogLevel logLevel = LogUtil.LogLevel.Info;

    public Boolean SepFile = true;

    private LuaSvr lua = null;
#if !UNITY_EDITOR
    private Dictionary<string, byte[]> luacache = new Dictionary<string, byte[]>();
#endif

    void RunApp()
    {
        SetupEnvironment();
        SetupPath();
        SetupLua();
    }

    void SetupEnvironment()
    {
        LogUtil.loglevel = logLevel;
        LogUtil.AttachUnityLogHandle();
        LogFile.Instance.Init();        
    }

    void SetupPath()
    {
        LogUtil.Log("AssetRoot:" + GameUtil.AssetRoot);
        LogUtil.Log("AssetsPath:" + GameUtil.AssetPath);
        LogUtil.Log("LuaPath:" + GameUtil.LuaPath);
#if ASYNC_MODE
        string assetBundlePath = GameUtil.AssetPath ;
        ResourceManager.BaseDownloadingURL = GameUtil.MakePathForWWW(assetBundlePath);
#if !UNITY_EDITOR
        if(SepFile)
            ResourceManager.PckPath = GameUtil.SepPath;
#endif
#else
        string assetBundlePath = GameUtil.AssetPath;
        string baseAssetURL = GameUtil.MakePathForWWW(assetBundlePath);
        ResourceManager.Instance.Initialize(baseAssetURL);
#endif
    }

    void SetupLua()
    {
        if (string.IsNullOrEmpty(EntryLuaScript))
            return;

        LuaState.loaderDelegate = loadLuaFile;
        lua = new LuaSvr();
        lua.init(null, () =>
        {
            string entryFile = GameUtil.MakePathForLua(EntryLuaScript);
            if(!Directory.Exists(GameUtil.AssetPath) || !File.Exists(entryFile))
            {
                StartCoroutine(_LoadStreamingAssets());
            }
            else
            {
                lua.start(EntryLuaScript);
            }
        }, SrvFlag);        
    }

    IEnumerator _LoadStreamingAssets()
    {
        string sourceFileName = AppConst.ZipName;
        string filename = GameUtil.AssetRoot + "/" + sourceFileName;

        byte[] bytes = null;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        string sourcepath = GameUtil.MakePathForWWW(Application.streamingAssetsPath + "/" + sourceFileName);
        LogUtil.Log("load asset from " + sourcepath);
        WWW www = new WWW(sourcepath);
        yield return www;
        if (www.error != null)
        {
            LogUtil.LogWarning(string.Format("Error _LoadStreamingAssets.Reason:{0}",www.error));
            yield break;
        }
        bytes = www.bytes;
#elif UNITY_IPHONE 
		string sourcepath = Application.dataPath + "/Raw/" + sourceFileName;
        LogUtil.Log("load asset from " + sourcepath);
		try{ 
			using ( FileStream fs = new FileStream(sourcepath, FileMode.Open, FileAccess.Read, FileShare.Read) )
            { 
				bytes = new byte[fs.Length]; 
				fs.Read(bytes,0,(int)fs.Length); 
			}   
		} 
        catch (System.Exception e)
        { 
            LogUtil.LogWarning(string.Format("Failed _LoadStreamingAssets.Reason:{0}",e.Message));
		} 
#elif UNITY_ANDROID 
		string sourcepath = "jar:file://" + Application.dataPath + "!/assets/"+sourceFileName; 			
		LogUtil.Log("load asset from " + sourcepath); 
		WWW www = new WWW(sourcepath); 
        yield return www;
        if (www.error != null)
        {           
            LogUtil.LogWarning(string.Format("Error _LoadStreamingAssets.Reason:{0}",www.error));
            yield break;
        }
		bytes = www.bytes; 
#endif

        if (bytes != null)
        {
            GameUtil.CreateDirectoryForFile(filename);
            // copy zip  file into cache folder 
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
                LogUtil.Log("Copy res form streaminAssets to persistentDataPath: " + filename);
            }
            yield return new WaitForEndOfFrame();

            //解压缩
            SharpZipUtil.SimpleUnZipFile(filename, GameUtil.AssetRoot + "/");
            LogUtil.Log(string.Format("Unpack {0} to {1}", sourceFileName, GameUtil.AssetRoot + "/"));

            yield return new WaitForEndOfFrame();

            //删除临时zip包
            File.Delete(filename);

            yield return new WaitForEndOfFrame();

            LogUtil.Log(string.Format("\n{0} created!  ", "res"));

            //加载入口文件
            lua.start(EntryLuaScript);
        }
    }

    byte[] loadLuaFile(string f)
    {
        string fn = f.ToLower();
        string luafilepath = GameUtil.MakePathForLua(fn);
        try
        {
            FileStream fs = File.Open(luafilepath, FileMode.Open);
            long length = fs.Length;
            byte[] bytes = new byte[length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();

            LogUtil.Log("loadfile:" + f);
            return bytes;
        }
        catch (Exception)
        {
            LogUtil.LogWarning("Cannot loadfile:" + luafilepath);
            return null;
        }
    }

    void Awake()
    {
        s_instance = this;
        DontDestroyOnLoad(gameObject);
        
        RunApp();
    }
#if TEST_EASYSOCKET
    SuperSocket.ClientEngine.FTestSuperSocket testSocket;
#endif
    // Use this for initialization
    void Start () {
#if TEST_EASYSOCKET
        testSocket = new SuperSocket.ClientEngine.FTestSuperSocket();
        testSocket.ConnectTo("127.0.0.1", 3001);
#endif
    }

    // Update is called once per frame
    void Update () {

	}

    void OnDestroy()
    {
        LogUtil.DetachUnityLogHandle();
        LogFile.Instance.UnInit();
    }

    // void OnApplicationPause()
    // {
    //     if (null == lua || null == lua.luaState)
    //         return;
    //     LuaState l = lua.luaState;
    //     LuaFunction func = l.getFunction("OnApplicationPause");
    //     if (null != func)
    //     {
    //         func.call();
    //         func.Dispose();
    //     }
    //     else
    //     {
    //         LogUtil.Log("OnApplicationPause");
    //     }
    // }

    // void OnApplicationFocus()
    // {
    //     if (null == lua || null == lua.luaState)
    //         return;
    //     LuaState l = lua.luaState;
    //     LuaFunction func = l.getFunction("OnApplicationFocus");
    //     if (null != func)
    //     {
    //         func.call();
    //         func.Dispose();
    //     }
    //     else
    //     {
    //         LogUtil.Log("OnApplicationFocus");
    //     }
    // }

    // void OnApplicationQuit()
    // {
    //     if (null == lua || null == lua.luaState)
    //         return;
    //     LuaState l = lua.luaState;
    //     LuaFunction func = l.getFunction("OnApplicationQuit");
    //     if (null != func)
    //     {
    //         func.call();
    //         func.Dispose();
    //     }
    //     else
    //     {
    //         LogUtil.Log("OnApplicationQuit");
    //     }
    // }
}
