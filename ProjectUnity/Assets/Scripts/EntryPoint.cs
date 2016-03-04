using UnityEngine;
using System.Collections;
using FLua;
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
        LogUtil.Log("AssetsPath:" + GameUtil.AssetPath);
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
#if !UNITY_EDITOR
    IEnumerator onLoadLuaBundle(Action complete)
    {
        string luaBundlePath = GameUtil.LuaPath ;
        string lua_bundle =  GameUtil.MakePathForWWW(luaBundlePath + AppConst.luabundle);
        WWW www = new WWW(lua_bundle);
        yield return www;    
        try
        {
            if (www.error == null)
            {
                AssetBundle item = www.assetBundle;
                string []keyNames = item.GetAllAssetNames();
                string str_temp = "Assets/" + AppConst.luatemp + "/";
                foreach (var ass in keyNames)
                {
                    string key = GameUtil.FileNameWithoutExt(GameUtil.RapFilePath(ass.ToLower()).Replace(str_temp.ToLower(), ""));     
                    luacache[key] = item.LoadAsset<TextAsset>(ass).bytes;
                }
                item.Unload(true);
                if (complete != null)
                {
                    complete();
                }
            }
            else
            {
                LogUtil.LogWarning(string.Format("error to load{0},reason:{1}",lua_bundle,www.error));
            }
        }
        catch(Exception e)
        {
            LogUtil.LogWarning(e.Message);
        }
    }
#endif
    void SetupLua()
    {
        if (string.IsNullOrEmpty(EntryLuaScript))
            return;

        LuaState.loaderDelegate = loadLuaFile;
        lua = new LuaSvr();
        lua.init(null, () =>
        {
#if !UNITY_EDITOR
            StartCoroutine(onLoadLuaBundle(() => { lua.start(EntryLuaScript); }));
#else
            lua.start(EntryLuaScript);
#endif
        }, SrvFlag);        
    }

    byte[] loadLuaFile(string f)
    {
        string fn = f.ToLower();
        byte[] s = null;
#if UNITY_EDITOR
        string luafilepath = GameUtil.MakePathForLua(fn);
        try
        {
            s = File.ReadAllBytes(luafilepath);
            LogUtil.Log("loadfile:" + f);
        }
        catch(Exception)
        {
            LogUtil.LogWarning("Cannot loadfile:" + luafilepath);
        }
#else
        string pckfile;
        if(SepFile && GameUtil.IsSepFileExist("Lua/" + fn + ".lua",out pckfile))
        {
            s = File.ReadAllBytes(pckfile);
            LogUtil.Log("loadfile:" + pckfile);
        }
        if (s == null && luacache.ContainsKey(fn))
        {
            s = luacache[fn];
            LogUtil.Log("loadfile:" + f);
        }
#endif
        return s;
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

    void OnApplicationPause()
    {
        if (null == LuaSvr.mainLuaState || null == LuaSvr.mainLuaState.luaState)
            return;
        LuaState l = LuaSvr.mainLuaState.luaState;
        LuaFunction func = l.getFunction("OnApplicationPause");
        if (null != func)
        {
            func.call();
            func.Dispose();
        }
        else
        {
            LogUtil.Log("OnApplicationPause");
        }
    }

    void OnApplicationFocus()
    {
        if (null == LuaSvr.mainLuaState || null == LuaSvr.mainLuaState.luaState)
            return;
        LuaState l = LuaSvr.mainLuaState.luaState;
        LuaFunction func = l.getFunction("OnApplicationFocus");
        if (null != func)
        {
            func.call();
            func.Dispose();
        }
        else
        {
            LogUtil.Log("OnApplicationFocus");
        }
    }

    void OnApplicationQuit()
    {
        if (null == LuaSvr.mainLuaState || null == LuaSvr.mainLuaState.luaState)
            return;
        LuaState l = LuaSvr.mainLuaState.luaState;
        LuaFunction func = l.getFunction("OnApplicationQuit");
        if (null != func)
        {
            func.call();
            func.Dispose();
        }
        else
        {
            LogUtil.Log("OnApplicationQuit");
        }
    }
}
