using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using System.Runtime.InteropServices;
using System;

namespace SLua
{
    public class LuaLPegDLL
    {

#if UNITY_IPHONE && !UNITY_EDITOR
	const string LUADLL = "__Internal";
#else
    const string LUADLL = "slua";
#endif

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_lpeg(IntPtr luaState);

        [SLua.Lua3rdDLL.LualibReg("lpeg")]
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int luaL_openlpeg(IntPtr l)
        {
            return luaopen_lpeg(l);
        }

        //public static void reg(Dictionary<string, LuaCSFunction> DLLRegFuncs)
        //{
        //    DLLRegFuncs.Add("lpeg", luaL_openlpeg);
        //}
    }
}
