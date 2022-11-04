/// <summary>
/// Author:WeiXin
/// Description:错误异常上报
/// Date: 2021-02-24 14:15:29
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class ExceptionReport : MonoBehaviour
{
    // [Header("捕捉日志类型")] 
    public static bool _Log = false;
    public static bool _Warning = false;
    public static bool _Assert = false;
    public static bool _Exception = true;
    public static bool _Error = true;

    public static bool IsReportLog = false;
    static private MultiKeyDictionary<string, string, Dictionary<string, string>> logCache;
    private static readonly Queue<Action> executionActionQueue = new Queue<Action>();
    private static readonly object lockObj = new object();
    private static ExceptionReport _instance = null;
    private static int isUploadAws = 1;//1:need upload
    private static string logFileName = String.Empty;
    private static string reportTag = "[log]";
    private static string stateUnity = "[UNITY]";
    private static string saveFileNameKey = "logFileName";
    private static string saveTime = "logSaveTime";
    private static string isUploadAwsKey = "isUploadAws";
    private static List<string> preLogs = new List<string>();
    public static void Init()
    {
#if !UNITY_EDITOR
        preLogs.Clear();
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            logCache = new MultiKeyDictionary<string, string, Dictionary<string, string>>();
            GameObject dis = new GameObject("ExceptionReport");
            _instance = dis.AddComponent<ExceptionReport>();
            _instance.gameObject.DontDestroy();
            Application.logMessageReceivedThreaded += LogMessageReceivedThreaded;
        }
#endif
    }
    
    private void Update()
    {
        lock (executionActionQueue)
        {
            while (executionActionQueue.Count > 0)
            {
                try
                {
                    executionActionQueue.Dequeue()?.Invoke();
                }
                catch (Exception e)
                {
                    LoggerUtils.Log("ExceptionReport executionQueue Error:" + e);
                }
            }
        }
    }

    void OnDestroy()
    {
        IsReportLog = false;
        preLogs.Clear();
        DisposeFileSteam();
        executionActionQueue.Clear();
        _instance = null;
#if !UNITY_EDITOR
        Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;
#endif
    }



    private static void LogMessageReceivedThreaded(string logString, string stackTrace, LogType type)
    {
        string extractStackTrace = string.Empty;
        switch (type)
        {
            case LogType.Error:
                if (!_Error)
                {
                    return;
                }
                extractStackTrace = StackTraceUtility.ExtractStackTrace();
                break;
            case LogType.Assert:
                if (!_Assert)
                {
                    return;
                }

                break;
            case LogType.Warning:
                if (!_Warning)
                {
                    return;
                }

                break;
            case LogType.Log:
                if (logString.Contains(reportTag))
                {
                    break;
                }
                if (!_Log)
                {
                    return;
                }

                break;
            case LogType.Exception:
                if (!_Exception)
                {
                    return;
                }
                extractStackTrace = StackTraceUtility.ExtractStackTrace();
                break;
            default:
                break;
        }
        ReportLog(logString,stackTrace,extractStackTrace,type);
        lock (executionActionQueue)
        {
            if (!logString.Contains(reportTag))
            {
                executionActionQueue.Enqueue(() => ReportLog(logString, stackTrace, extractStackTrace));
            }
        }
        
    }

    private static StreamWriter sWrite;

    private static void ReportLog(string logString, string stackTrace,string extractStackTrace, LogType type)
    {
        if (IsReportLog)
        {
            lock (lockObj)
            {
                try
                {
                    if ((type == LogType.Log && logString.Contains(reportTag)) || type == LogType.Exception ||
                        type == LogType.Error)
                    {
                        WriteLogFile(logString, stackTrace, extractStackTrace, type == LogType.Exception);
                    }
                }
                catch (Exception e)
                {
                    IsReportLog = false;
                    DisposeFileSteam();
                    LoggerUtils.LogError("ReportLog Error logString=" + logString + " e.Message =" + e.Message);
                }
            }
        }
    }

    public static void DisposeFileSteam()
    {
        if (sWrite != null)
        {
            sWrite.Close();
            sWrite.Dispose();
            sWrite = null;
        }
    }
    
    /// <summary>
    /// 只在创建Log文件前使用
    /// </summary>
    /// <param name="message"></param>
    /// <param name="eveName"></param>
    public static void LogPreReport(object message,string eveName)
    {
        string curTime = GameUtils.GetCurTimeStr();
        string logString = string.Format("[log][{0}]{1}[t]{2}", eveName, message, curTime);
        preLogs.Add(logString);
    }

    public static void CreateReportLogOnGameStart()
    {
        //关闭log开关,删除所有log信息
        if (!IsReportLog)
        {
            DeleteLogFileOnClose();
            return;
        }

        string version = "0.0.0";
        if (GameManager.Inst.unityConfigInfo != null)
        {
            version = GameManager.Inst.unityConfigInfo.appVersion;
        }
        string enr = "master";
        if ((GameManager.Inst.baseGameJsonData != null && GameManager.Inst.baseGameJsonData.baseInfo != null))
        {
            enr = GameManager.Inst.baseGameJsonData.baseInfo.environment;
        }
        logFileName =GameInfo.Inst.myUid+"-"+ GameUtils.GetTimeDay() +"-"+version+ ".txt";
        UploadLogToAws(version,enr);
        CheckOldLogFile(logFileName);
        WriteTitle();
        PlayerPrefs.SetInt(isUploadAwsKey, 1);
    }

    private static void WriteTitle()
    {
        sWrite.WriteLine(stateUnity);
        sWrite.WriteLine("[uid]{0}", GameInfo.Inst.myUid);
        string mapId = GameManager.Inst.gameMapInfo == null ? "" : GameManager.Inst.gameMapInfo.mapId;
        sWrite.WriteLine("[mapId]{0}",mapId);
        if ((GameManager.Inst.baseGameJsonData != null && GameManager.Inst.baseGameJsonData.baseInfo != null))
        {
            string device = GameManager.Inst.baseGameJsonData.baseInfo.device;
            string generation = GameManager.Inst.baseGameJsonData.baseInfo.generation ?? "";
            if (!string.IsNullOrEmpty(device))
            {
                sWrite.WriteLine("[device]{0}--{1}", device, generation);
            }
        }

        if (GameManager.Inst.unityConfigInfo != null &&
            !string.IsNullOrEmpty(GameManager.Inst.unityConfigInfo.callUnityTimeStamp))
        {
            if (double.TryParse(GameManager.Inst.unityConfigInfo.callUnityTimeStamp,out double timpStamp))
            {
                string callTime = GameUtils.GetTimeStrByStamp(timpStamp);
                sWrite.WriteLine("[log]Call Unity[t]{0}", callTime);
            }
        }

        for (var i = 0; i < preLogs.Count; i++)
        {
            sWrite.WriteLine(preLogs[i]);
            sWrite.Flush();
        }
        preLogs.Clear();
    }

    private static void CheckOldLogFile(string newFileName)
    {
        if (PlayerPrefs.HasKey(saveFileNameKey))
        {
            string saveFileName = PlayerPrefs.GetString(saveFileNameKey);
            string saveFullPath = DataUtils.logDataDir + saveFileName;
            if (!saveFileName.Equals(newFileName) && File.Exists(saveFullPath))
            {
                File.Delete(saveFullPath);
            }
           
        }
        string filePath = DataUtils.logDataDir + newFileName;
        try
        {
            if (!Directory.Exists(DataUtils.logDataDir))
            {
                Directory.CreateDirectory(DataUtils.logDataDir);
            }

            DisposeFileSteam();
            sWrite = new StreamWriter(filePath, true);
        }
        catch (Exception e)
        {
            DisposeFileSteam();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            IsReportLog = false;
            LoggerUtils.LogError("LogFile Create Error: " + e.Message);
            return;
        }

        PlayerPrefs.SetString(saveTime,GameUtils.GetTimeDay());
        PlayerPrefs.SetString(saveFileNameKey,newFileName);
    }

    private static void DeleteLogFileOnClose()
    {
        if (PlayerPrefs.HasKey(saveFileNameKey))
        {
            string saveFileName = PlayerPrefs.GetString(saveFileNameKey);
            string saveFullPath = DataUtils.logDataDir + saveFileName;
            if (File.Exists(saveFullPath))
            {
                File.Delete(saveFullPath);
            }
            PlayerPrefs.DeleteKey(saveFullPath);
        }
        if (PlayerPrefs.HasKey(saveTime))
        {
            PlayerPrefs.DeleteKey(saveTime);
        }
        if (PlayerPrefs.HasKey(isUploadAwsKey))
        {
            PlayerPrefs.DeleteKey(isUploadAwsKey);
        }
    }

    private static void UploadLogToAws(string version,string enr)
    {
        if (!PlayerPrefs.HasKey(saveFileNameKey))
        {
            return;
        }
        string saveFileName = PlayerPrefs.GetString(saveFileNameKey);
        string saveFullPath = DataUtils.logDataDir + saveFileName;

        if (!File.Exists(saveFullPath))
        {
            return;
        }
        FileInfo fInfo=new FileInfo(saveFullPath);

        if (fInfo.Length == 0)
        {
            return;
        }
        if (PlayerPrefs.HasKey(isUploadAwsKey) && PlayerPrefs.GetInt(isUploadAwsKey) == 1)
        {
            string awsFileName = Path.GetFileNameWithoutExtension(saveFileName)+ "_1.txt";
            string uploadFile =  DataUtils.logDataDir + awsFileName;

            if (!Directory.Exists(DataUtils.logDataDir))
            {
                return;
            }
            if (File.Exists(uploadFile))
            {
                File.Delete(uploadFile);
            }
            string sTime = PlayerPrefs.HasKey(saveTime) ? PlayerPrefs.GetString(saveTime) : GameUtils.GetTimeDay();
            string platform = (Application.platform == RuntimePlatform.IPhonePlayer) ? "IOS" : "Android";
            string awsDir = String.Format("U3D/Log/{0}/{1}/{2}/{3}/", enr, platform, sTime, version);
            File.Copy(saveFullPath,uploadFile,true);
            AWSUtill.UpLoadRes(awsFileName, uploadFile, awsDir, (msg) =>
            {
            }, (x) =>
            {
                Debug.LogError("LogFile ExceptionReport Fail");

            }, true);
        }
    }

    private static void WriteLogFile(string logString, string stackTrack, string extractStackTrace, bool isStack)
    {
        if (!IsReportLog || sWrite == null)
        {
            return;
        }

        if (logString.Contains(reportTag))
        {
            sWrite.WriteLine(logString);
        }
        else
        {
            string curTime = GameUtils.GetCurTimeStr();
            sWrite.WriteLine("[log]{0}[t]{1}",logString, curTime);
            if (isStack)
            {
                sWrite.WriteLine("[stackTrace]{0}", stackTrack.Trim());
                sWrite.WriteLine("[extractStackTrace]{0}", extractStackTrace.Trim());
            }
        }
        sWrite.Flush();
    }

    public static void RemoveLastReportOnGameSuccess()
    {
        if (!IsReportLog)
        {
            return;
        }
        if (sWrite != null)
        {
            DisposeFileSteam();
        }

        string logFullPath = DataUtils.logDataDir + logFileName;
        if (File.Exists(logFullPath))
        {
            var contents = File.ReadAllLines(logFullPath);
            var lastIndex = Array.FindLastIndex(contents, x => x.Equals(stateUnity));
            if (lastIndex >= 0)
            {
                var nContents = new string[lastIndex];
                Array.Copy(contents,nContents,lastIndex);
                File.WriteAllLines(logFullPath,nContents);
            }
        }
        PlayerPrefs.SetInt(isUploadAwsKey, 0);
        IsReportLog = false;
    }

    static void ReportLog(string logString, string stackTrace, string extractStackTrace)
    {
        var _log = new Dictionary<string, string>()
        {
            {"uid", GameInfo.Inst.myUid},
            {"log", logString},
            {"stackTrace", stackTrace},
            {"extractStackTrace", extractStackTrace},
            {"mapId", GameManager.Inst.gameMapInfo == null ? string.Empty : GameManager.Inst.gameMapInfo.mapId},
        };

        if (!logCache.ContainsKey(logString, stackTrace))
        {
            logCache[logString][stackTrace] = _log;
            MobileInterface.Instance.ExceptionReport(JsonConvert.SerializeObject(_log));
        }
    }
}