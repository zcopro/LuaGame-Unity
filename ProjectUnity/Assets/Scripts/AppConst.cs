using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AppConst
{
    public static string ip = "0.0.0.0";
    public static int port = 10240;

    public const string AppName = "FLuaGame";           //应用程序名称
    public const string AppPrefix = AppName + "_";             //应用程序前缀
    public const string ExtName = ".assetbundle";              //素材扩展名
    public const string AssetDirname = "StreamingAssets";      //素材目录 
    public const string luabundle = "lua.assetbundle";         //lua bundle
    public const string luaExt = ".bytes";                     //
    public const string luatemp = "LuaTemp";                   //temp floder for bundle
    public const string WebUrl = "http://localhost:6688/";      //测试更新地址

    public static string UserId = string.Empty;                 //用户ID
}
