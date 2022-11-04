using System;
using System.IO;
using System.Text;
using BudEngine.NetEngine.src.Util.Def;
using UnityEngine;

namespace BudEngine.NetEngine.src.Util
{
    public static class Debugger
    {
        public static bool Enable = false;
        public static Action Callback = null;
        public static bool isSaveLog = false;
        public static void Log(string format, params object[] args)
        {
            if (!Enable)
                return;
            // Console.WriteLine(String.Format(format, args)); 

            string str;
            try
            {
                str = "[" + RequestHeader.Version + "] " + string.Format(format, args);
            }
            catch (Exception)
            {
                str = "[" + RequestHeader.Version + "] " + format;
            }

            if (isSaveLog)
            {
                lock (sb)
                {
                    string outputStr = string.Format("[{0}:{1}:{2}]--{3}", DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond, str);
                    sb.AppendLine(outputStr);
                    Debug.Log(string.Format("<color=#00ff00>{0}</color>", outputStr));
                }
            }
            else
            {
                Debug.Log(string.Format("<color=#00ff00>{0}</color>", str));
            }

            Callback?.Invoke();
        }

        public static void LogNormal(string format, params object[] args)
        {
            if (!Enable)
                return;
            // Console.WriteLine(String.Format(format, args)); 

            string str;
            try
            {
                str = "[" + RequestHeader.Version + "] " + string.Format(format, args);
            }
            catch (Exception)
            {
                str = "[" + RequestHeader.Version + "] " + format;
            }

            if (isSaveLog)
            {
                lock (sb)
                {
                    string outputStr = string.Format("[{0}:{1}:{2}]--{3}", DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond, str);
                    sb.AppendLine(outputStr);
                    Debug.Log(outputStr);
                }
            }
            else
            {
                Debug.Log(str);
            }

            Callback?.Invoke();
        }

        [NonSerialized]
        private static string saveLogPath = Application.dataPath + "/Debugger.log";
        private static StringBuilder sb = new StringBuilder();
        public static void SaveLogFile()
        {
            if (!Enable || !isSaveLog)
                return;

            if (sb != null && !string.IsNullOrEmpty(sb.ToString()))
            {
                Debugger.Log("Save Log Finish:" + saveLogPath);
                File.WriteAllText(saveLogPath, sb.ToString());
            }
        }
    }
}