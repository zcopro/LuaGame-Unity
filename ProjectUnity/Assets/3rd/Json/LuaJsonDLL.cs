using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using System.Runtime.InteropServices;
using System;

namespace FLua
{
    public class LuaJsonDLL
    {

#if UNITY_IPHONE && !UNITY_EDITOR
	const string LUADLL = "__Internal";
#else
    const string LUADLL = "FLua";
#endif

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_cjson(IntPtr luaState);

        [FLua.Lua3rdDLL.LualibReg("cjson")]
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int luaL_opencjson(IntPtr l)
        {
            return luaopen_cjson(l);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_cjson_safe(IntPtr luaState);

        [FLua.Lua3rdDLL.LualibReg("cjson.safe")]
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int luaL_opencjson_safe(IntPtr l)
        {
            return luaopen_cjson_safe(l);
        }

        //public static void reg(Dictionary<string, LuaCSFunction> DLLRegFuncs)
        //{
        //    DLLRegFuncs.Add("cjson", luaL_opencjson);
        //    DLLRegFuncs.Add("cjson.safe", luaL_opencjson_safe);
        //}
    }
}
