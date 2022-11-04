using BudEngine.NetEngine.src.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: 熊昭
/// Description: unity-log日志管理器
/// Date: 2021-11-29 15:41:48
/// </summary>
public class LoggerUtils
{
    private static string tag = "UnityMsgLog--";
    public static bool IsDebug = true;
    
    public static void Log(object message)
    {
        if (!IsDebug) return;

        if (Debugger.isSaveLog) 
            Debugger.LogNormal(tag + message);
        else 
            Debug.Log(tag + message);
    }

    public static void LogError(object message)
    {
        if (!IsDebug) return;
        Debug.LogError(tag + message);

        if (Debugger.isSaveLog)
            Debugger.LogNormal(tag + message);
    }

    public static void LogReport(object message,string eveName = "")
    {
        if (!IsDebug || !ExceptionReport.IsReportLog) return;
        string eve = string.IsNullOrEmpty(eveName) ?"": string.Format("[{0}]", eveName);
        string curTime = GameUtils.GetCurTimeStr();
        Debug.Log(string.Format("[log]{0}{1}[t]{2}", eve, message,curTime));
    }
}