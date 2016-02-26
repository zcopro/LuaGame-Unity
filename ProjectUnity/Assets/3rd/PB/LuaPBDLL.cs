using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using System.Runtime.InteropServices;
using System;

namespace SLua
{
    public class LuaPBDLL
    {

#if UNITY_IPHONE && !UNITY_EDITOR
	const string LUADLL = "__Internal";
#else
    const string LUADLL = "slua";
#endif

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_pb(IntPtr luaState);

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int luaL_openpb(IntPtr l)
        {
            return luaopen_pb(l);
        }

        public static void reg(Dictionary<string, LuaCSFunction> DLLRegFuncs)
        {
            DLLRegFuncs.Add("pb", luaL_openpb);
        }
    }
}
