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
        const string LUADLL = "FLua";
#endif

        public static void Establish(IntPtr L)
        {
            FLua_EstablishAnyLog(On_FLua_AnyLog);
            FLua_SetupLuaState(L);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FLua_CleanupLuaState();

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FLua_SetupLuaState(IntPtr luaState);

        public delegate void FLua_AnyLog_Delegate(LogType logType, string message);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public extern static void FLua_EstablishAnyLog(FLua_AnyLog_Delegate func);

        [AOT.MonoPInvokeCallback(typeof(FLua_AnyLog_Delegate))]
        private static void On_FLua_AnyLog(LogType logType, string message)
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
