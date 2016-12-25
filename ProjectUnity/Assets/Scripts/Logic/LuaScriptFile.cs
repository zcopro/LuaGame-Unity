using UnityEngine;
using System.Collections;
using SLua;

[CustomLuaClass]
public class LuaScriptFile : MonoBehaviour {

	LuaState lua = null;
	LuaTable script = null;

	bool bRequired = false;
	public string scriptFileName ;

    public bool usingUpdate = false;
    public bool usingFixedUpdate = false;
    public bool usingLateUpdate = false;
    public bool forceUpdate = false;

	public LuaState env {
		get {
			if(lua == null && LuaSvr.main != null && LuaSvr.main.inited){
				lua = LuaSvr.main.luaState;
			}
			return lua;
		}
		set {
			lua = value;
		}
	}

	public string Buffer {
		get { 
			return scriptFileName;
		}
		set {
			if(scriptFileName != value)
			{
				Cleanup();
				scriptFileName = value;
                DoScriptFile(() =>
                {
                    CallMethod("Start");
                });
            }
		}
	}

	// Use this for initialization
	void Start () {
		if(!bRequired){
			DoScriptFile(()=>
            {
                CallMethod("Start");
            });
		}		
	}
	
	// Update is called once per frame
	void Update () {
		if(!usingUpdate && !forceUpdate)
			return;
		if(forceUpdate){
			forceUpdate = false;
		}
		CallMethod("Update",Time.deltaTime);
	}

	void FixedUpdate () {
		if(!usingFixedUpdate)
			return;
		CallMethod("FixedUpdate");
	}

	void LateUpdate () {
		if(!usingLateUpdate)
			return;
		CallMethod("LateUpdate");
	}

	void OnDestroy(){
		CallMethod("OnDestroy");

        Cleanup();
	}

    IEnumerator DoFile(string fn,System.Action complete = null)
    {
        yield return new WaitForEndOfFrame();
        while (env == null )
        {
            yield return new WaitForEndOfFrame();
        }
        if (!string.IsNullOrEmpty(fn) && env != null)
        {
            bRequired = true;
            object obj = env.doFile(fn);
            if (obj != null && obj is LuaTable)
            {
                script = (LuaTable)obj;

                script["this"] = this;
                script["transform"] = transform;
                script["gameObject"] = gameObject;
            }
        }
        if (complete != null)
            complete();
    }

	void DoScriptFile(System.Action complete = null){
        StartCoroutine(DoFile(scriptFileName, complete));
	}

	void Cleanup(){
		if (!bRequired)
			return;
		bRequired = false;
		if (script != null)
        {
            script.Dispose();
            script = null;
        }
	}


	protected object CallMethod(string function, params object[] args)
    {
    	if(!bRequired) return null;
        if (script == null || script[function] == null || !(script[function] is LuaFunction))
        	return null;

        LuaFunction func = (LuaFunction)script[function];

        if (func == null) return null;
        try
        {
            object ret = null;
            if (args != null)
                ret = func.call(args);
            else
                ret = func.call();
            return ret;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(FormatException(e), gameObject);
        }
        return null;
    }

    protected object CallMethod(string function)
    {
        return CallMethod(function, null);
    }

    public static string FormatException(System.Exception e)
    {
        string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
        return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
    }
}
