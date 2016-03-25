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
        const string LUADLL = "slua";
#endif

        public static void Establish(IntPtr L)
        {
            L_EstablishAnyLog(On_SLua_AnyLog);
            L_SetupLuaState(L);
        }

        public static void UnEstablish()
        {
            L_UnEstablishAnyLog();
            L_CleanupLuaState();
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int L_CleanupLuaState();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int L_SetupLuaState(IntPtr luaState);

        public delegate void SLua_AnyLog_Delegate(LogType logType, string message);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public extern static void L_EstablishAnyLog(SLua_AnyLog_Delegate func);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public extern static void L_UnEstablishAnyLog();

        [AOT.MonoPInvokeCallback(typeof(SLua_AnyLog_Delegate))]
        private static void On_SLua_AnyLog(LogType logType, string message)
        {
            switch(logType)
            {
                case LogType.Log:
                    Debug.Log("["+LUADLL+"]"+message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning("["+LUADLL+"]"+message);
                    break;
                case LogType.Error:
                    Debug.LogError("["+LUADLL+"]"+message);
                    break;
                case LogType.Exception:
                    Debug.LogException(new Exception("["+LUADLL+"]"+message));
                    break;
                case LogType.Assert:
                    Debug.LogError("["+LUADLL+"]"+message);
                    break;
            }
        }
    }
}
