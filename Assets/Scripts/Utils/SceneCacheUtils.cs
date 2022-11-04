using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Author:LiShuZhan
/// Description:进房时长优化，将解压出的json保存起来，每次解压前会判断此处是否有json，有则直接加载
/// Date: 2022.02.23
/// </summary>
public class SceneCacheUtils : MonoBehaviour
{
    public static string sceneCacheDir => Application.persistentDataPath + "/U3D/SceneCache/";
    public const int maxJsonNum = 19;
    public static void SaveSceneCacheJson(string fileName, string content)
    {
        string tempName = fileName + ".json";
        string fullScPath = Path.Combine(sceneCacheDir, tempName);
        if (!Directory.Exists(sceneCacheDir))
        {
            Directory.CreateDirectory(sceneCacheDir);
        }
        FileStream sceneCachefs = File.Create(fullScPath);
        FileInfo fileInfo = new FileInfo(fullScPath);
        StreamWriter writer = new StreamWriter(sceneCachefs);
        writer.Write(content);
        writer.Dispose();
        sceneCachefs.Dispose();
        AddSceneCacheConfig(fileName, fileInfo.GetHashCode(), fullScPath);
    }

    public static string LoadSceneCacheJson(string mapName)
    {
        if (Directory.Exists(sceneCacheDir))
        {
            DirectoryInfo direction = new DirectoryInfo(sceneCacheDir);
            FileInfo[] files = direction.GetFiles("*.json");
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.Replace(".json", "") == mapName)
                {
                    string fullPath = Path.Combine(sceneCacheDir, files[i].Name);
                    string content = File.ReadAllText(fullPath);
                    return content;
                }
            }
        }
        return null;
    }


    public static string LoadNativeCacheJson(string mapName)
    {
        var nativeCacheSavePath = OfflineResManager.Inst.nativeCacheSavePath;
        if (Directory.Exists(nativeCacheSavePath))
        {
            DirectoryInfo direction = new DirectoryInfo(nativeCacheSavePath);

            // 优先处理 zip 文件
            // LoggerUtils.Log("######################## nativeJson  mapName = " + mapName + " ##############");
            string fullPath = Path.Combine(nativeCacheSavePath, mapName + ".zip");
            // LoggerUtils.Log("######################## nativeJson  fullPath = " + fullPath + " ##############");
            if (File.Exists(fullPath))
            {
                byte[] jsonBytes = File.ReadAllBytes(fullPath);
                string content = ZipUtils.SaveZipFromByte(jsonBytes);
                if (!string.IsNullOrEmpty(content))
                {
                    // 本地保存 Json
                    SceneCacheUtils.SaveSceneCacheJson(mapName, content);
                    LoggerUtils.Log("######################## nativeJson  zip file exits ##############");
                    return content;
                }
            }

            // 没有 zip 文件情况下，看看是否有 Json (由于线上的一些比较老的地图没有 zip，只有 Json)
            string jsonFullPath = Path.Combine(nativeCacheSavePath, mapName + ".json");
            // LoggerUtils.Log("######################## nativeJson  jsonFullPath = " + jsonFullPath + " ##############");
            if (File.Exists(jsonFullPath))
            {
                string content = File.ReadAllText(jsonFullPath);
                if (!string.IsNullOrEmpty(content))
                {
                    // 本地保存 Json
                    SceneCacheUtils.SaveSceneCacheJson(mapName, content);
                    LoggerUtils.Log("######################## nativeJson  Json file exits ##############");
                    return content;
                }

            }
        }
        return null;
    }

    public static void CheckSceneCacheNum()
    {
        if (Directory.Exists(sceneCacheDir))
        {
            DirectoryInfo direction = new DirectoryInfo(sceneCacheDir);
            FileInfo[] files = direction.GetFiles("*.json");
            if (files.Length < 1) { return; }
            FileInfo oldFile = files[0];
            if (files.Length > maxJsonNum)
            {
                foreach (var x in files)
                {
                    if (x.CreationTime.CompareTo(oldFile.CreationTime) < 0)
                    {
                        oldFile = x;
                    }
                }
                File.Delete(oldFile.FullName);
                ReSceneCacheConfig();
            }
        }
    }

    private static void ReSceneCacheConfig()
    {
        string tempName = "SceneCacheConfig.txt";
        string fullPach = Path.Combine(sceneCacheDir, tempName);
        DirectoryInfo direction = new DirectoryInfo(sceneCacheDir);
        File.WriteAllText(fullPach, string.Empty);
        FileInfo[] files = direction.GetFiles("*.json");
        foreach (var x in files)
        {
            AddSceneCacheConfig(x.Name, x.GetHashCode(), x.FullName);
        }
    }

    public static void AddSceneCacheConfig(string scName, int hashCode, string fullScPach)
    {
        string tempName = "SceneCacheConfig.txt";
        string fullPach = Path.Combine(sceneCacheDir, tempName);
        if (!File.Exists(fullPach))
        {
            FileInfo fileTemp = new FileInfo(fullPach);
            fileTemp.CreateText().Dispose();
        }
        byte[] data = File.ReadAllBytes(fullScPach);
        FileInfo file = new FileInfo(fullPach);
        StreamWriter sw = file.AppendText();
        sw.WriteLine(scName + "|" + hashCode + "|" + data.Length);
        sw.Dispose();
    }
}
