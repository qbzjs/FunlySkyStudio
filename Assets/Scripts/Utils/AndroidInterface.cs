using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidInterface:CInstance<AndroidInterface>
{
    //MapScene Activity
    static AndroidJavaObject javaObject2;
    //RoleScene Activity
    private static AndroidJavaObject javaObject3;
    static readonly string androidClassPath2 = "com.pointone.buddyglobal.feature.unity.view.UnityPlayerActivity";
    static readonly string AndroidClassPaht3 = "com.pointone.buddyglobal.feature.unity.view.UnityPersonImageActivity";
    public static void Call(string funcName, string msg)
    {
        try
        {
            var jo = GetObject();
            jo.Call(funcName, msg);
        }
        catch(Exception e)
        {
            LoggerUtils.LogError($"Unity Error = Failed to invoke Interface Android Call err: {e}");
        }

    }

    public static void RoleCall(string funcName, string msg)
    {
        try
        {
            var jo = GetRoleObject();
            jo.Call(funcName, msg);
        }
        catch
        {
            LoggerUtils.LogError("Unity Error = Failed to invoke Interface Android RollCall");
        }
    }


    public static AndroidJavaObject GetRoleObject()
    {
        if (javaObject3 == null)
        {
            AndroidJavaClass jc3 = new AndroidJavaClass(AndroidClassPaht3);
            javaObject3 = jc3.CallStatic<AndroidJavaObject>("getInstance");
        }
        return javaObject3;
    }

    public static AndroidJavaObject GetObject()
    {
        if (javaObject2 == null)
        {
            AndroidJavaClass jc2 = new AndroidJavaClass(androidClassPath2);
            javaObject2 = jc2.CallStatic<AndroidJavaObject>("getInstance");
        }
        return javaObject2;
    }

    public override void Release()
    {
        base.Release();
        javaObject2 = null;
        javaObject3 = null;
    }
}
