#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityLog = UnityEngine.Debug;

public class Packager {

    [MenuItem("Pack/步骤1.打包AssetBundle/Build iPhone Resource", false, 11)]
    public static void BuildiPhoneResource() {
        string output = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/", "");
        if (output.Length == 0)
            return;
        output += "/StreamingAssets/iOS/StreamingAssets/";
        BuildAssetResource(output,BuildTarget.iOS, false);
    }

    [MenuItem("Pack/步骤1.打包AssetBundle/Build OSX Resource", false, 12)]
    public static void BuildMacResource()
    {
        string output = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/", "");
        if (output.Length == 0)
            return;
        output += "/StreamingAssets/OSX/StreamingAssets/";
        BuildAssetResource(output, BuildTarget.StandaloneOSXIntel, false);
    }

    [MenuItem("Pack/步骤1.打包AssetBundle/Build Android Resource", false, 13)]
    public static void BuildAndroidResource() {
        string output = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/", "");
        if (output.Length == 0)
            return;
        output += "/StreamingAssets/Android/StreamingAssets/";
        BuildAssetResource(output,BuildTarget.Android, true);
    }

    [MenuItem("Pack/步骤1.打包AssetBundle/Build Windows Resource", false, 14)]
    public static void BuildWindowsResource() {
        string output = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/", "");
        if (output.Length == 0)
            return;

        output += "/StreamingAssets/Windows/StreamingAssets/";
        BuildAssetResource(output,BuildTarget.StandaloneWindows, true);
    }

    [MenuItem("Assets/Bundle Name/Attach", false, 15)]
    public static void SetAssetBundleName()
    {
        UnityEngine.Object[] selection = Selection.objects;

        AssetImporter import = null;
        foreach (Object s in selection)
        {
            import = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(s));
            import.assetBundleName = s.name + AppConst.ExtName;
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Bundle Name/Detah", false, 16)]
    public static void ClearAssetBundleName()
    {
        Object[] selection = Selection.objects;

        AssetImporter import = null;
        foreach (Object s in selection)
        {
            import = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(s));
            import.assetBundleName = null;
        }
        AssetDatabase.Refresh();
    }

    static void _CreateAssetBunldesMain(string targetPath)
    {
        //获取在Project视图中选择的所有游戏对象
        Object[] SelectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        //遍历所有的游戏对象
        foreach (Object obj in SelectedAsset)
        {
            //string sourcePath = AssetDatabase.GetAssetPath(obj);
            //本地测试：建议最后将Assetbundle放在StreamingAssets文件夹下，如果没有就创建一个，因为移动平台下只能读取这个路径
            //StreamingAssets是只读路径，不能写入
            //服务器下载：就不需要放在这里，服务器上客户端用www类进行下载。           
            if (BuildPipeline.BuildAssetBundle(obj, null, (targetPath + obj.name + ".assetbundle").ToLower(), BuildAssetBundleOptions.CollectDependencies))
            {
                UnityLog.Log(obj.name + "资源打包成功");
            }
            else
            {
                UnityLog.Log(obj.name + "资源打包失败");
            }
        }
        //刷新编辑器
        AssetDatabase.Refresh();

    }

    [MenuItem("Assets/Create AssetBunldes Main")]
    static void CreateAssetBunldesMain()
    {
        string dst_path = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/StreamingAssets/", "");
        if (dst_path.Length == 0)
            return;

        string targetPath = dst_path + "/";
        _CreateAssetBunldesMain(targetPath);
    }
    
    /// <summary>
    /// 生成绑定素材
    /// </summary>
    public static void BuildAssetResource(string path,BuildTarget target, bool isWin) {
        UnityLog.Log(string.Format("打包资源到目录:{0},平台:{1}", path,target.ToString()));

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, target);

        AssetDatabase.Refresh();
        UnityLog.Log("AssetBundle打包完成");
    }

    static string GetLuaSrcPath()
    {
        string appPath = Application.dataPath.ToLower();
        return appPath.Replace("assets", "") + "../Output/StreamingAssets/Lua/";
    }
    static string GetResourceSrcPath()
    {
        string appPath = Application.dataPath.ToLower();
        return appPath.Replace("assets", "") + "../Output/";
    }

    static void _EncodeLuaFile(string srcFile, string outFile, bool isWin)
    {
        if (!srcFile.ToLower().EndsWith(".lua")) {
            return;
        }
        string appPath = Application.dataPath.ToLower();
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            luaexe = "luajit.exe";
            args = "-b " + srcFile + " " + outFile;
            exedir = appPath.Replace("assets", "") + "../tools/LuaEncoder/luajit/";
        } else if (Application.platform == RuntimePlatform.OSXEditor) {
            luaexe = "./luac";
            args = "-o " + outFile + " " + srcFile;
            exedir = appPath.Replace("assets", "") + "../tools/LuaEncoder/luavm/";
        }

        UnityLog.Log("EncodeLuaFile:" + srcFile + "==>" + outFile);

        Directory.SetCurrentDirectory(exedir);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.UseShellExecute = isWin;
        info.ErrorDialog = true;
        UnityLog.Log(info.FileName + " " + info.Arguments);

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    static void _PackResFiles(string res, string outpath, bool isWin)
    {
        List<string> files = new List<string>();
        GameUtil.Recursive(ref files, res, new string[] { ".meta" });

        for (int i = 0; i < files.Count; ++i)
        {
            string newfile = files[i].Replace(res, "");
            string newpath = outpath + newfile;
            string path = Path.GetDirectoryName(newpath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.Copy(files[i], newpath, true);
        }
    }

    public static void cleanMeta(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta"))
            {
                File.Delete(@filename);
            }

            foreach (string dir in dirs)
            {
                cleanMeta(dir);
            }
        }
    }

    [MenuItem("Pack/步骤2.打成一个Zip包")]
    public static void PackUnityRes()
    {
        string dst_res = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/", "");
        if (dst_res.Length == 0)
            return;

        dst_res += "/StreamingAssets/" + AppConst.ZipName;

        string src_res = GetResourceSrcPath();

        string src_export = src_res + "StreamingAssets/";
        //剔除manifest        
        foreach (string filename in Directory.GetFiles(src_export,"*.manifest", SearchOption.AllDirectories))
        {
            File.Delete(filename);
        }

        SharpZipUtil.SimpleZipFile(dst_res, src_export);

        UnityLog.Log("Zip包自作完成,path=" + dst_res);
    }

    static void CopyDirTo(string src,string dst,bool delete = false)
    {
        string[] fileTempList = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories);
        foreach(string s in fileTempList)
        {
            if (!s.EndsWith(".meta"))
            {
                string fileName = s.Replace(src, "");
                string outFile = Path.Combine(dst, fileName);
                string dir_path = Path.GetDirectoryName(outFile);
                if (!Directory.Exists(dir_path))
                    Directory.CreateDirectory(dir_path);
                File.Copy(s, outFile,true);

                if(delete)
                {
                    File.Delete(s);
                }
            }
        }
    }

    static void CopyLuaToPath(string dir,bool isWin,bool encoder = false,string ext = ".bytes")
    {
        string src_path = GetLuaSrcPath();
        List<string> src_files = new List<string>();
        GameUtil.Recursive(ref src_files, src_path, null);
        for(int i=0;i<src_files.Count;++i)
        {
            string newfile = src_files[i].Replace(src_path, "");
            string newfilename = GameUtil.FileNameWithoutExt(newfile) + ext;
            string newfilepath = dir + newfilename;
            string path = Path.GetDirectoryName(newfilepath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (encoder)
                _EncodeLuaFile(src_files[i], newfilepath, isWin);
            else
                File.Copy(src_files[i], newfilepath,true);
        }
        AssetDatabase.Refresh();
    }

    static void _BuildLuaBundle(string dst_path,bool isWin,bool encoder = false)
    {
        string dir_path = Path.GetDirectoryName(dst_path);
        if (!Directory.Exists(dir_path))
            Directory.CreateDirectory(dir_path);

        string appPath = Application.dataPath.ToLower();
        string temp_path = appPath + "/" + AppConst.luatemp + "/";
        string bundle_path = temp_path + "bundle/";
        CopyLuaToPath(temp_path, isWin,encoder, AppConst.luaExt);

        if (!Directory.Exists(Path.GetDirectoryName(bundle_path)))
            Directory.CreateDirectory(Path.GetDirectoryName(bundle_path));

        string[] buildTempList = Directory.GetFiles(temp_path, "*" + AppConst.luaExt, SearchOption.AllDirectories);

        List<string> buildList = new List<string>();
        for(int i=0;i<buildTempList.Length;++i)
        {
            string assetPath = "Assets" + buildTempList[i].Replace(appPath, "");
            buildList.Add(assetPath);
        }
        UnityLog.Log("total files:" + buildTempList.Length);

        //List<Object> list_files = new List<Object>();
        //for (int i = 0; i < buildList.Count; ++i)
        //{
        //    string assetPath = buildList[i];
        //    Object obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
        //    if (obj != null)
        //        list_files.Add(obj);
        //    else
        //        UnityLog.LogWarning("can not LoadMainAssetAtPath:" + assetPath);
        //}

        //if (list_files.Count > 0)
        //{
        //    BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle;
        //    string bundleName = AppConst.luabundle;
        //    string output_bundle = bundle_path + bundleName;
        
        //    BuildPipeline.BuildAssetBundle(null, list_files.ToArray(), output_bundle, options, EditorUserBuildSettings.activeBuildTarget);

        //    if (Directory.Exists(temp_path)) Directory.Delete(temp_path, true);
        //    AssetDatabase.Refresh();
        //}

        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = AppConst.luabundle;
        build.assetNames = buildList.ToArray();
        AssetDatabase.Refresh();

        BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle |
                                          BuildAssetBundleOptions.UncompressedAssetBundle;
        BuildPipeline.BuildAssetBundles(bundle_path, new AssetBundleBuild[] { build }, options, EditorUserBuildSettings.activeBuildTarget);

        CopyDirTo(bundle_path, dst_path);

        if (Directory.Exists(temp_path)) Directory.Delete(temp_path, true);
        AssetDatabase.Refresh();

        UnityLog.Log("lua package build ok.");
    }

    [MenuItem("Pack/Pack Lua Bundle No Encoder")]
    public static void BuildLuaAssetBundle()
    {
        string dst_path = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/StreamingAssets/", "");
        if (dst_path.Length == 0)
            return;

        dst_path += "/StreamingAssets/";

        _BuildLuaBundle(dst_path,true,false);
    }

    [MenuItem("Pack/Pack Lua Bundle With Encoder")]
    public static void BuildLuaAssetBundleWithEncoder()
    {
        string dst_path = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/StreamingAssets/", "");
        if (dst_path.Length == 0)
            return;

        dst_path += "/StreamingAssets/";
        _BuildLuaBundle(dst_path,true,true);
    }
}
#endif