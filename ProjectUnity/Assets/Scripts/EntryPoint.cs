using UnityEngine;
using System.Collections;
using FLua;
using System.IO;
using FGame.Utility;
using FGame.Manager;
using System;
using System.Collections.Generic;

public class EntryPoint : MonoBehaviour {

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
        SetupPath();
        SetupLua();
    }

    void SetupPath()
    {
        LogUtil.Log("AssetsPath:" + Util.DataPath);
#if ASYNC_MODE
        ResourceManager.BaseDownloadingURL = Util.GetRelativePath();
#if !UNITY_EDITOR
        if(SepFile)
            ResourceManager.PckPath = Util.SepPath;
#endif
#else
        string baseAssetURL = Util.GetRelativePath();
        ResourceManager.Instance.Initialize(baseAssetURL);
#endif
    }
#if !UNITY_EDITOR
    IEnumerator onLoadLuaBundle(Action complete)
    {
        string lua_bundle = Util.GetRelativePath() + AppConst.luabundle;
        WWW www = new WWW(lua_bundle);
        yield return www;
        if (www.error == null)
        {
            AssetBundle item = www.assetBundle;
            string []keyNames = item.GetAllAssetNames();
            string str_temp = "Assets/" + AppConst.luatemp + "/";
            foreach (var ass in keyNames)
            {
                string key = Util.FileNameWithoutExt(Util.RapFilePath(ass.ToLower()).Replace(str_temp.ToLower(), ""));     
                luacache[key] = item.LoadAsset<TextAsset>(ass).bytes;
            }
            item.Unload(true);
            if (complete != null)
            {
                complete();
            }
        }
    }
#endif
    void SetupLua()
    {
        if (string.IsNullOrEmpty(EntryLuaScript))
            return;

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
        fn += ".lua";
        string luafilepath = Util.LuaPath(fn);
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
        if(SepFile && Util.IsSepFileExist("Lua/" + fn + ".lua",out pckfile))
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
        LogUtil.loglevel = logLevel;
        LogUtil.AttachUnityLogHandle();
        DontDestroyOnLoad(gameObject);
        LuaState.loaderDelegate = loadLuaFile;

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
    }
}
