

using System;

namespace SLua
{
#if SLUA_STANDALONE
	internal enum LogType
	{
		Error,
		Assert,
		Warning,
		Log,
		Exception
	}
#endif

    /// <summary>
    /// A bridge between UnityEngine.Debug.LogXXX and standalone.LogXXX
    /// </summary>
    internal class Logger
    {
#if SLUA_STANDALONE
		public delegate void LogCallback (string condition, string stackTrace, LogType type);
		public static event LogCallback logMessageReceived;
#endif
        public static void Log(string msg)
        {
#if !SLUA_STANDALONE
            UnityEngine.Debug.Log(msg);
#else
            Console.WriteLine(msg);
			if(logMessageReceived != null)
				logMessageReceived(msg, "", LogType.Log);
#endif 
        }
        public static void LogError(string msg)
        {
#if !SLUA_STANDALONE
            UnityEngine.Debug.LogError(msg);
#else
            Console.WriteLine(msg);
			if(logMessageReceived != null)
				logMessageReceived(msg, "", LogType.Error);
#endif
        }

		public static void LogWarning(string msg)
		{
#if !SLUA_STANDALONE
			UnityEngine.Debug.LogWarning(msg);
#else
            Console.WriteLine(msg);
			if(logMessageReceived != null)
				logMessageReceived(msg, "", LogType.Warning);
#endif
		}

		public static void LogException(System.Exception ex, UnityEngine.Object context = null)
		{
#if !SLUA_STANDALONE
			if (null == context)
				UnityEngine.Debug.LogException(ex);
			else
				UnityEngine.Debug.LogException(ex, context);
#else
			if(null == context)
				Console.WriteLine("Exception:" + ex.Message);
			else
				Console.WriteLine("Exception:" + ex.Message + ", " + context.ToString());

			if(logMessageReceived != null)
				logMessageReceived(ex.Message, "", LogType.Exception);
#endif
		}
    }
}