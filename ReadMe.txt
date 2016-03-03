public delegate void UnityLogDelegate(LogType logType,string log);

[DllImport("slua")
public extern static void FLua_InitLog(UnityLogDelegate func);

[AOT.MonoPInvokeCallback(typeof(UnityLogDelegate))]
private void onUnityLog(LogType logType,string log)
{
}