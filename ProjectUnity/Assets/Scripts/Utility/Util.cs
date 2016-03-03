using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using FLua;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FGame.Utility
{
    [CustomLuaClass]
    public class Util {
        public static int Int(object o) {
            return Convert.ToInt32(o);
        }

        public static float Float(object o) {
            return (float)Math.Round(Convert.ToSingle(o), 2);
        }

        public static long Long(object o) {
            return Convert.ToInt64(o);
        }

        public static int Random(int min, int max) {
            return UnityEngine.Random.Range(min, max);
        }

        public static float Random(float min, float max) {
            return UnityEngine.Random.Range(min, max);
        }

        public static string Uid(string uid) {
            int position = uid.LastIndexOf('_');
            return uid.Remove(0, position + 1);
        }

        public static long GetTime() {
            TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            return (long)ts.TotalMilliseconds;
        }

        public static string ToHexString(byte[] bytes, string sep = ",")
        {
            string byteStr = string.Empty;
            if (bytes != null || bytes.Length > 0)
            {
                int nPos = 0;
                foreach (var item in bytes)
                {
                    nPos++;
                    byteStr += string.Format("{0:X2}", item);
                    if (nPos < bytes.Length)
                        byteStr += sep;
                }
            }
            return byteStr;
        }

        public static string ToBytesString(byte[] bytes, string sep = ",")
        {
            string byteStr = string.Empty;
            if (bytes != null || bytes.Length > 0)
            {
                int nPos = 0;
                foreach (var item in bytes)
                {
                    nPos++;
                    byteStr += item.ToString();
                    if (nPos < bytes.Length)
                        byteStr += sep;
                }
            }
            return byteStr;
        }

        /// <summary>
        /// 搜索子物体组件-GameObject版
        /// </summary>
        [DoNotToLua]
        public static T Get<T>(GameObject go, string subnode) where T : Component {
            if (go != null) {
                Transform sub = go.transform.FindChild(subnode);
                if (sub != null) return sub.GetComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// 搜索子物体组件-Transform版
        /// </summary>
        [DoNotToLua]
        public static T Get<T>(Transform go, string subnode) where T : Component {
            if (go != null) {
                Transform sub = go.FindChild(subnode);
                if (sub != null) return sub.GetComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// 搜索子物体组件-Component版
        /// </summary>
        [DoNotToLua]
        public static T Get<T>(Component go, string subnode) where T : Component {
            return go.transform.FindChild(subnode).GetComponent<T>();
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        [DoNotToLua]
        public static T Add<T>(GameObject go) where T : Component {
            if (go != null) {
                T[] ts = go.GetComponents<T>();
                for (int i = 0; i < ts.Length; i++) {
                    if (ts[i] != null) GameObject.Destroy(ts[i]);
                }
                return go.gameObject.AddComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        [DoNotToLua]
        public static T Add<T>(Transform go) where T : Component {
            return Add<T>(go.gameObject);
        }

        /// <summary>
        /// 查找子对象
        /// </summary>
        public static GameObject Child(GameObject go, string subnode) {
            return Child(go.transform, subnode);
        }

        /// <summary>
        /// 查找子对象
        /// </summary>
        public static GameObject Child(Transform go, string subnode) {
            Transform tran = go.FindChild(subnode);
            if (tran == null) return null;
            return tran.gameObject;
        }

        /// <summary>
        /// 取平级对象
        /// </summary>
        public static GameObject Peer(GameObject go, string subnode) {
            return Peer(go.transform, subnode);
        }

        /// <summary>
        /// 取平级对象
        /// </summary>
        public static GameObject Peer(Transform go, string subnode) {
            Transform tran = go.parent.FindChild(subnode);
            if (tran == null) return null;
            return tran.gameObject;
        }

        /// <summary>
        /// 手机震动
        /// </summary>
        public static void Vibrate() {
            //int canVibrate = PlayerPrefs.GetInt(Const.AppPrefix + "Vibrate", 1);
            //if (canVibrate == 1) iPhoneUtils.Vibrate();
        }

        /// <summary>
        /// Base64编码
        /// </summary>
        public static string Encode(string message) {
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(message);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        public static string Decode(string message) {
            byte[] bytes = Convert.FromBase64String(message);
            return Encoding.GetEncoding("utf-8").GetString(bytes);
        }

        /// <summary>
        /// 判断数字
        /// </summary>
        public static bool IsNumeric(string str) {
            if (str == null || str.Length == 0) return false;
            for (int i = 0; i < str.Length; i++) {
                if (!Char.IsNumber(str[i])) { return false; }
            }
            return true;
        }

        /// <summary>
        /// HashToMD5Hex
        /// </summary>
        public static string HashToMD5Hex(string sourceStr) {
            byte[] Bytes = Encoding.UTF8.GetBytes(sourceStr);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider()) {
                byte[] result = md5.ComputeHash(Bytes);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                    builder.Append(result[i].ToString("x2"));
                return builder.ToString();
            }
        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string md5(string source) {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = "";
            for (int i = 0; i < md5Data.Length; i++) {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string md5file(string file) {
            try {
                FileStream fs = new FileStream(file, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++) {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            } catch (Exception ex) {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        /// <summary>
        /// 清除所有子节点
        /// </summary>
        public static void ClearChild(Transform go) {
            if (go == null) return;
            for (int i = go.childCount - 1; i >= 0; i--) {
                GameObject.Destroy(go.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 生成一个Key名
        /// </summary>
        public static string GetKey(string key) {
            return AppConst.AppPrefix + AppConst.UserId + "_" + key;
        }

        /// <summary>
        /// 取得整型
        /// </summary>
        public static int GetInt(string key) {
            string name = GetKey(key);
            return PlayerPrefs.GetInt(name);
        }

        /// <summary>
        /// 有没有值
        /// </summary>
        public static bool HasKey(string key) {
            string name = GetKey(key);
            return PlayerPrefs.HasKey(name);
        }

        /// <summary>
        /// 保存整型
        /// </summary>
        public static void SetInt(string key, int value) {
            string name = GetKey(key);
            PlayerPrefs.DeleteKey(name);
            PlayerPrefs.SetInt(name, value);
        }

        /// <summary>
        /// 取得数据
        /// </summary>
        public static string GetString(string key) {
            string name = GetKey(key);
            return PlayerPrefs.GetString(name);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public static void SetString(string key, string value) {
            string name = GetKey(key);
            PlayerPrefs.DeleteKey(name);
            PlayerPrefs.SetString(name, value);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        public static void RemoveData(string key) {
            string name = GetKey(key);
            PlayerPrefs.DeleteKey(name);
        }

        /// <summary>
        /// 是否为数字
        /// </summary>
        public static bool IsNumber(string strNumber) {
            Regex regex = new Regex("[^0-9]");
            return !regex.IsMatch(strNumber);
        }

        /// <summary>
        /// 取得数据存放目录
        /// </summary>
        public static string DataPath {
            get {
#if UNITY_EDITOR
                return Application.dataPath + "/../../Output/" + AppConst.AssetDirname + "/";
#elif UNITY_IPHONE
                return Application.dataPath + "/";
#elif UNITY_ANDRIOD
                return Application.persistentDataPath + "/";
#elif UNITY_STANDALONE
                return Application.streamingAssetsPath + "/";
#else
                return Application.dataPath + "/";
#endif
            }
        }

        public static string GetRelativePath() {
            if (Application.isMobilePlatform || Application.isConsolePlatform)
                return "file:///" + DataPath;
            else
                return "file://" + DataPath;
        }

        /// <summary>
        /// 应用程序内容路径
        /// </summary>
        public static string AppContentPath() {
            string path = string.Empty;
            switch (Application.platform) {
                case RuntimePlatform.Android:
                    path = "jar:file://" + Application.dataPath + "!/assets/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    path = Application.dataPath + "/Raw/";
                    break;
                default:
                    path = "file://" + Application.dataPath + "/" + AppConst.AssetDirname + "/";
                    break;
            }
            return path;
        }

        /// <summary>
        /// 取得Lua路径
        /// </summary>
        public static string LuaPath(string name) {
#if UNITY_EDITOR
            string path = DataPath + "../lua/";
#else
            string path = DataPath;
#endif
            string lowerName = name.ToLower();
            if (lowerName.EndsWith(".lua")) {
                int index = name.LastIndexOf('.');
                name = name.Substring(0, index);
            }
            name = name.Replace('.', '/');
            path = path + name + ".lua";
            return path;
        }

        public static string MakeRelativePath(string path)
        {
            if (Application.isMobilePlatform || Application.isConsolePlatform)
                return "file:///" + path;
            else
                return "file://" + path;
        }

        public static string GetRelativePckPath()
        {
            if (Application.isMobilePlatform || Application.isConsolePlatform)
                return "file:///" + SepPath;
            else
                return "file://" + SepPath;
        }

        public static string SepPath
        {
            get
            {
                return DataPath + "pck/";
            }
        }
        public static bool IsSepFileExist(string f,out string filename)
        {
            string fn = f.ToLower();
            if (File.Exists(SepPath + fn))
            {
                filename = SepPath + fn;
                return true;
            }
            else
            {
                filename = null;
                return false;
            }
        }
        /// <summary>
        /// CreateDirectory
        /// </summary>
        /// <param name="dir"></param>
        public static void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        public static bool IsDirectoryExist(string dir)
        {
            return Directory.Exists(dir);
        }
        
        public static string RapFilePath(string fullname)
        {
            return fullname.Replace('\\', '/');
        }

        public static string GetFilePath(string fullname)
        {
            String temp = RapFilePath(fullname);

            int nPos = temp.LastIndexOf('/');
            if (nPos != -1)
            {
                return temp.Substring(0,nPos);
            }
            else
                return temp;
        }
        public static string FileName(string fullname)
        {
            String temp = RapFilePath(fullname);

            int nPos = temp.LastIndexOf('/');
            if (nPos != -1)
            {
                return temp.Substring(nPos + 1);
            }
            else
                return temp;
        }
        public static string FileNameWithoutExt(string fullname)
        {
            int nPos = fullname.LastIndexOf('.');
            if (nPos != -1)
            {
                return fullname.Substring(0,nPos);
            }
            else
                return fullname;
        }
        static bool ContainsFileExt(string[] filters, string ext)
        {
            for (int i = 0; i < filters.Length; ++i)
            {
                if (filters[i].Equals(ext))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 遍历目录及其子目录
        /// </summary>
        [DoNotToLua]
        public static void Recursive(ref List<string> files, string path, string[] filters)
        {
            try
            {
                string[] names = Directory.GetFiles(path);
                string[] dirs = Directory.GetDirectories(path);
                foreach (string filename in names)
                {
                    string ext = Path.GetExtension(filename);
                    if (filters != null && ContainsFileExt(filters, ext)) continue;
                    files.Add(filename.Replace('\\', '/'));
                }
                foreach (string dir in dirs)
                {
                    Recursive(ref files, dir, filters);
                }
            }
            catch(Exception e)
            {
                LogUtil.LogWarning(e.Message);
            }
        }

        public static List<string> RecursiveFiles(string path, string[] filters)
        {
            List<string> ret = new List<string>();
            Recursive(ref ret, path, filters);
            return ret;
        }


        /// <summary>
        /// 是不是苹果平台
        /// </summary>
        /// <returns></returns>
        public static bool isApplePlatform {
            get {
                return Application.platform == RuntimePlatform.IPhonePlayer ||
                       Application.platform == RuntimePlatform.OSXEditor ||
                       Application.platform == RuntimePlatform.OSXPlayer;            
            }
        }

        public static bool isWindowPlatform
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.WindowsPlayer ||
                    Application.platform == RuntimePlatform.WindowsWebPlayer;
            }
        }
    }
}