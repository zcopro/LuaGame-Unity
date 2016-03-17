using UnityEngine;
using System.Collections;
using SLua;
public class LuaScript : MonoBehaviour {

	LuaState lua = null;
	LuaTable script = null;

	bool bRequired = false;
	public string buffer ;

	[System.NonSerialized]
    public bool usingUpdate = false;
    [System.NonSerialized]
    public bool usingFixedUpdate = false;
    [System.NonSerialized]
    public bool usingLateUpdate = false;
    [System.NonSerialized]
    public bool forceUpdate = false;

	public LuaState env {
		get {
			if(lua == null && LuaSvr.mainLuaState != null){
				lua = LuaSvr.mainLuaState.luaState;
			}
			return lua;
		}
		set {
			lua = value;
		}
	}

	public string Buffer {
		get { 
			return buffer;
		}
		set {
			if(buffer ~= value)
			{
				Cleanup();
				buffer = value;
				LoadBuffer();
				CallMethod("Start");
			}
		}
	}

	// Use this for initialization
	void Start () {
		if(!bRequired){
			LoadBuffer();
		}
		CallMethod("Start");
	}
	
	// Update is called once per frame
	void Update () {
		if(!usingUpdate && !forceUpdate)
			return;
		if(forceUpdate){
			forceUpdate = false;
		}
		CallMethod("Update");
	}

	void FixedUpdate () {
		if(!usingFixedUpdate)
			return;
		CallMethod("FixedUpdate");
	}

	void LateUpdate () {
		if(!LateUpdate)
			return;
		CallMethod("LateUpdate");
	}

	void OnDestroy(){
		CallMethod("OnDestroy");

        Cleanup();
	}

	void LoadBuffer(){
		if(!string.IsNullOrEmpty(buffer) && env != null){
			bRequired = true;
			object obj = env.doString(buffer);
			if (obj != null){
				script = (LuaTable)obj;

				script["this"] = this;
		        script["transform"] = transform;
		        script["gameObject"] = gameObject;
			}
		}
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
            if (args != null)
            {
                return func.call(args);
            }
            return func.call();
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
