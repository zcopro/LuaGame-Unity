namespace LuaInterface
{
    using System;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Collections;
    using System.Text;
    using System.Security;
    using UnityEngine;

    public class LuaDLLNativeRuntime
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		const string LUADLL = "__Internal";
#else
        const string LUADLL = "SLua";
#endif

        public static void Establish(IntPtr L)
        {
            SLua_EstablishAnyLog(On_SLua_AnyLog);
            SLua_SetupLuaState(L);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SLua_CleanupLuaState();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SLua_SetupLuaState(IntPtr luaState);

        public delegate void SLua_AnyLog_Delegate(LogType logType, string message);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SLua_EstablishAnyLog(SLua_AnyLog_Delegate func);

        [AOT.MonoPInvokeCallback(typeof(SLua_AnyLog_Delegate))]
        private static void On_SLua_AnyLog(LogType logType, string message)
        {
            switch(logType)
            {
                case LogType.Log:
                    Debug.Log(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Exception:
                    Debug.LogException(new Exception(message));
                    break;
                case LogType.Assert:
                    Debug.LogError(message);
                    break;
            }
        }
    }
}
