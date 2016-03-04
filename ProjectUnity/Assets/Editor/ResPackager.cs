#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Packager {

    [MenuItem("Game/Build iPhone Resource", false, 11)]
    public static void BuildiPhoneResource() {
        string output = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/StreamingAssets/", "");
        if (output.Length == 0)
            return;
        output += "/StreamingAssets/";
        BuildAssetResource(output,BuildTarget.iOS, false);
    }

    [MenuItem("Game/Build Android Resource", false, 12)]
    public static void BuildAndroidResource() {
        string output = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/StreamingAssets/", "");
        if (output.Length == 0)
            return;
        output += "/StreamingAssets/";
        BuildAssetResource(output,BuildTarget.Android, true);
    }

    [MenuItem("Game/Build Windows Resource", false, 13)]
    public static void BuildWindowsResource() {
        string output = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/StreamingAssets/", "");
        if (output.Length == 0)
            return;

        output += "/StreamingAssets/";
        BuildAssetResource(output,BuildTarget.StandaloneWindows, true);
    }

    [MenuItem("Game/Bundle Name/Attach", false, 14)]
    [MenuItem("Assets/Bundle Name/Attach", false, 14)]
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

    [MenuItem("Game/Bundle Name/Detah", false, 14)]
    [MenuItem("Assets/Bundle Name/Detah", false, 14)]
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
                UnityEngine.Debug.Log(obj.name + "资源打包成功");
            }
            else
            {
                UnityEngine.Debug.Log(obj.name + "资源打包失败");
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
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, target);

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("BuildAssetResource Success,target="+target.ToString());
    }

    static string GetLuaSrcPath()
    {
        string appPath = Application.dataPath.ToLower();
        return appPath.Replace("assets", "") + "../Output/Lua/";
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

        UnityEngine.Debug.Log("EncodeLuaFile:" + srcFile + "==>" + outFile);

        Directory.SetCurrentDirectory(exedir);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.UseShellExecute = isWin;
        info.ErrorDialog = true;
        UnityEngine.Debug.Log(info.FileName + " " + info.Arguments);

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

    [MenuItem("Pack/Pack Unity Resource")]
    public static void PackUnityRes()
    {
        string dst_res = EditorUtility.OpenFolderPanel("Build Assets ", "Assets/StreamingAssets/", "");
        if (dst_res.Length == 0)
            return;

        dst_res += "/";
        string src_res = GetResourceSrcPath();

        _PackResFiles(src_res, dst_res, true);
    }

    static void CopyDirTo(string src,string dst)
    {
        string[] fileTempList = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories);
        foreach(string s in fileTempList)
        {
            if (!s.EndsWith(".meta"))
            {
                string fileName = s.Replace(src, "");
                string outFile = dst + fileName;
                string dir_path = Path.GetDirectoryName(outFile);
                if (!Directory.Exists(dir_path))
                    Directory.CreateDirectory(dir_path);
                File.Copy(s, outFile,true);
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
        UnityEngine.Debug.Log("total files:" + buildTempList.Length);

        //List<Object> list_files = new List<Object>();
        //for (int i = 0; i < buildList.Count; ++i)
        //{
        //    string assetPath = buildList[i];
        //    Object obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
        //    if (obj != null)
        //        list_files.Add(obj);
        //    else
        //        UnityEngine.Debug.LogWarning("can not LoadMainAssetAtPath:" + assetPath);
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

        UnityEngine.Debug.Log("lua package build ok.");
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