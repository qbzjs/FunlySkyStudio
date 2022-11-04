using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class ResPathTool
{
    public const string U3DResPath = "U3D/Res/";
    private static Dictionary<string, string> _envMapping = new Dictionary<string, string>()
    {
        { "master", $"{U3DResPath}Master" },
        { "pr", $"{U3DResPath}Alpha" },
        { "prod", $"{U3DResPath}Prod" }
    };



    
    public static string GetResUrlPrefix()
    {
        return $"{AWSUtill.urlRoot}{U3DResPath}";
    }

    public static string GetResUrlPrefixByEnv()
    {
#if UNITY_EDITOR
        return $"{AWSUtill.urlRoot}{ABUpLoadConfig.Instance.currentEnv}/";
#else
        string envK = "prod";
        if ((GameManager.Inst.baseGameJsonData != null && GameManager.Inst.baseGameJsonData.baseInfo != null))
        {
            var k = GameManager.Inst.baseGameJsonData.baseInfo.environment;
            if (_envMapping.ContainsKey(k))
            {
                envK = k;
            }
        }
        return $"{AWSUtill.urlRoot}{_envMapping[envK]}/";
#endif
        
    }

    public static string GetBundleFileList()
    {
        return $"AssetBundle/{GetPlatform()}/BundleFileList";
    }
    
    public static string GetBundleUrlPrefix()
    {
        return $"{GetResUrlPrefix()}Bundle/{GetPlatform()}";
    }
    
    public static string GetConfigUrlPrefix()
    {
        return $"{GetResUrlPrefixByEnv()}VersionConfig/";
    }
    
    public static string GetConfigPersistentDataPath()
    {
        return Path.Combine(Application.persistentDataPath, "ResConfig");
    }
    
    public static string GetConfigPersistentDataPathFile()
    {
        return Path.Combine(GetConfigPersistentDataPath(), "ConfigVersions.json");
    }
    
    public static string GetPlatform()
    {
#if UNITY_ANDROID
            return "Android";
#else 
            return "iOS";
#endif
    }

    
    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            StringBuilder sb = new StringBuilder();
            using (System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }
}
